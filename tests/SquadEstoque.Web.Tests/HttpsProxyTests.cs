using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class HttpsProxyTests(HttpsProxyTests.ProductionFactory factory)
    : IClassFixture<HttpsProxyTests.ProductionFactory>
{
    private const string PublicHost = "estoque.example.test";
    private static readonly IPAddress TrustedProxy = IPAddress.Parse("192.0.2.10");

    [Theory]
    [InlineData("192.0.2.10", "http")]
    [InlineData("192.0.2.11", "https")]
    [InlineData("127.0.0.1", "https")]
    [InlineData("192.0.2.10", "https, http")]
    public async Task Http_redirect_preserves_public_url_and_ignores_untrusted_scheme(string peer, string proto)
    {
        var response = await factory.Server.SendAsync(context =>
        {
            context.Connection.RemoteIpAddress = IPAddress.Parse(peer);
            context.Request.Host = new(PublicHost);
            context.Request.Path = "/health";
            context.Request.QueryString = new("?probe=1");
            context.Request.Headers["X-Forwarded-Proto"] = proto;
            context.Request.Headers["X-Forwarded-Host"] = "outside.example.test";
        });

        Assert.Equal(307, response.Response.StatusCode);
        Assert.Equal($"https://{PublicHost}/health?probe=1", response.Response.Headers.Location.ToString());
        Assert.False(response.Response.Headers.ContainsKey("Strict-Transport-Security"));
    }

    [Fact]
    public async Task Trusted_https_keeps_login_redirect_and_authenticated_return_on_public_host()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri($"https://{PublicHost}"),
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Add("X-Forwarded-Proto", "https");
        client.DefaultRequestHeaders.Add("X-Forwarded-Host", "outside.example.test");

        using var challenge = await client.GetAsync("/Produtos?probe=1");
        Assert.Equal(HttpStatusCode.Found, challenge.StatusCode);
        Assert.Equal($"https://{PublicHost}/Account/Login?ReturnUrl=%2FProdutos%3Fprobe%3D1",
            challenge.Headers.Location?.AbsoluteUri);
        Assert.StartsWith("max-age=", Assert.Single(challenge.Headers.GetValues("Strict-Transport-Security")));

        using var loginPage = await client.GetAsync(challenge.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, loginPage.StatusCode);
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        Assert.NotEmpty(token);

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "lojista@estoque.example.test",
            ["Senha"] = "SenhaDeTeste123!",
            ["ReturnUrl"] = "/Produtos?probe=1",
            ["__RequestVerificationToken"] = WebUtility.HtmlDecode(token)
        });
        using var login = await client.PostAsync(challenge.Headers.Location, form);
        Assert.Equal(HttpStatusCode.Found, login.StatusCode);
        Assert.Equal("/Produtos?probe=1", login.Headers.Location?.OriginalString);
        var cookie = Assert.Single(login.Headers.GetValues("Set-Cookie"),
            value => value.StartsWith(".AspNetCore.Cookies=", StringComparison.Ordinal));
        var attributes = cookie.Split(';', StringSplitOptions.TrimEntries);
        Assert.Contains("secure", attributes, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("httponly", attributes, StringComparer.OrdinalIgnoreCase);

        using var destination = await client.GetAsync(login.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, destination.StatusCode);
    }

    public sealed class ProductionFactory : WebApplicationFactory<Program>
    {
        private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory("estoque-https-");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AllowedHosts"] = PublicHost,
                ["ConnectionStrings:EstoqueContext"] = $"Data Source={Path.Combine(_directory.FullName, "Estoque.db")};Pooling=False",
                ["DataProtection:KeysPath"] = Path.Combine(_directory.FullName, "keys"),
                ["ReverseProxy:KnownProxies:0"] = TrustedProxy.ToString()
            }));
            builder.ConfigureServices(services =>
            {
                services.AddHttpsRedirection(options => options.HttpsPort = 443);
                services.AddSingleton<IStartupFilter, ProxyTransport>();
            });
        }

        protected override Microsoft.Extensions.Hosting.IHost CreateHost(Microsoft.Extensions.Hosting.IHostBuilder builder)
        {
            var host = base.CreateHost(builder);
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            db.Usuario.Add(new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = "Lojista Teste",
                Email = "lojista@estoque.example.test",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("SenhaDeTeste123!"),
                Perfil = PerfilUsuario.LOJISTA
            });
            db.SaveChanges();
            return host;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && _directory.Exists)
            {
                _directory.Delete(recursive: true);
            }
        }

        private sealed class ProxyTransport : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
            {
                app.Use(async (context, continuation) =>
                {
                    context.Connection.RemoteIpAddress ??= TrustedProxy;
                    context.Request.Scheme = "http";
                    await continuation();
                });
                next(app);
            };
        }
    }
}
