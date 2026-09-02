using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class AuthenticationAuthorizationTests : IClassFixture<SquadEstoqueWebApplicationFactory>
{
    private readonly SquadEstoqueWebApplicationFactory _factory;

    public AuthenticationAuthorizationTests(SquadEstoqueWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_page_contains_expected_form()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/Account/Login");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<form", html);
        Assert.Contains("method=\"post\"", html);
        Assert.Contains("name=\"Email\"", html);
        Assert.Contains("name=\"Senha\"", html);
        Assert.Contains("name=\"__RequestVerificationToken\"", html);
    }

    [Fact]
    public async Task Lojista_can_login_and_access_produtos()
    {
        using var client = CreateClient();
        await LoginAsync(client, "lojista@squad.com");

        var response = await client.GetAsync("/Produtos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Lojista_can_access_movimentacoes()
    {
        using var client = CreateClient();
        await LoginAsync(client, "lojista@squad.com");

        var response = await client.GetAsync("/Movimentacoes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Vendedor_can_login()
    {
        using var client = CreateClient();
        await LoginAsync(client, "vendedor@squad.com");

        var response = await client.GetAsync("/Account/Login");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/Estoque/Consulta", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Vendedor_can_access_saida()
    {
        using var client = CreateClient();
        await LoginAsync(client, "vendedor@squad.com");

        var response = await client.GetAsync("/Movimentacoes/Saida");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Vendedor_is_redirected_to_access_denied_for_produtos()
    {
        using var client = CreateClient();
        await LoginAsync(client, "vendedor@squad.com");

        var response = await client.GetAsync("/Produtos");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal(
            "/Account/AccessDenied?ReturnUrl=%2FProdutos",
            response.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task Invalid_credentials_do_not_authenticate()
    {
        using var client = CreateClient();
        var loginPage = await client.GetAsync("/Account/Login");
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiforgeryToken(html);
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "usuario.invalido@example.test",
            ["Senha"] = "senha-incorreta",
            ["__RequestVerificationToken"] = token
        });

        var loginResponse = await client.PostAsync("/Account/Login", content);
        var produtosResponse = await client.GetAsync("/Produtos");

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Found, produtosResponse.StatusCode);
        Assert.Equal(
            "/Account/Login?ReturnUrl=%2FProdutos",
            produtosResponse.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task Invalid_login_does_not_render_submitted_password()
    {
        using var client = CreateClient();
        var loginPage = await client.GetAsync("/Account/Login");
        var token = ExtractAntiforgeryToken(await loginPage.Content.ReadAsStringAsync());
        const string submittedPassword = "senha-secreta-nao-deve-voltar";
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "usuario.invalido@example.test",
            ["Senha"] = submittedPassword,
            ["__RequestVerificationToken"] = token
        });

        var response = await client.PostAsync("/Account/Login", content);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain(submittedPassword, html, StringComparison.Ordinal);
        Assert.Contains("E-mail ou senha inválidos.", WebUtility.HtmlDecode(html));
    }

    [Fact]
    public void Seeded_users_use_bcrypt_with_work_factor_12()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var users = context.Usuario.AsNoTracking().ToList();

        Assert.NotEmpty(users);
        Assert.All(users, user =>
        {
            Assert.NotEqual("123", user.SenhaHash);
            Assert.Matches(@"^\$2[aby]\$12\$", user.SenhaHash);
            Assert.True(BCrypt.Net.BCrypt.Verify("123", user.SenhaHash));
            Assert.False(BCrypt.Net.BCrypt.Verify("senha-incorreta", user.SenhaHash));
        });
    }

    [Fact]
    public async Task Logout_clears_authentication_session()
    {
        using var client = CreateClient();
        await LoginAsync(client, "lojista@squad.com");
        var produtosResponse = await client.GetAsync("/Produtos");
        var html = await produtosResponse.Content.ReadAsStringAsync();
        var token = ExtractAntiforgeryToken(html);
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token
        });

        var logoutResponse = await client.PostAsync("/Account/Logout", content);
        var afterLogoutResponse = await client.GetAsync("/Produtos");

        Assert.Equal(HttpStatusCode.OK, produtosResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Found, logoutResponse.StatusCode);
        Assert.Equal("/Account/Login", logoutResponse.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.Found, afterLogoutResponse.StatusCode);
        Assert.Equal(
            "/Account/Login?ReturnUrl=%2FProdutos",
            afterLogoutResponse.Headers.Location?.PathAndQuery);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    private static async Task LoginAsync(HttpClient client, string email)
    {
        var loginPage = await client.GetAsync("/Account/Login");
        loginPage.EnsureSuccessStatusCode();
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiforgeryToken(html);
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Senha"] = "123",
            ["__RequestVerificationToken"] = token
        });

        var response = await client.PostAsync("/Account/Login", content);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        var expectedDestination = email == "vendedor@squad.com"
            ? "/Estoque/Consulta"
            : "/Produtos";
        Assert.Equal(expectedDestination, response.Headers.Location?.OriginalString);
    }

    private static string ExtractAntiforgeryToken(string html)
    {
        var match = Regex.Match(
            html,
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"[^>]*>",
            RegexOptions.IgnoreCase);

        Assert.True(match.Success, "Token antiforgery não encontrado no formulário de login.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }
}
