using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class SecurityCoverageHttpTests
{
    private static readonly string[] AdminGets =
    [
        "/Produtos", "/Produtos/Details/00000000-0000-0000-0000-000000000000",
        "/Produtos/AdicionarNumeracoes/00000000-0000-0000-0000-000000000000",
        "/Produtos/Create", "/Produtos/Edit/00000000-0000-0000-0000-000000000000",
        "/Movimentacoes", "/Movimentacoes/Entrada", "/Movimentacoes/EntradaLote?produtoId=00000000-0000-0000-0000-000000000000",
        "/Movimentacoes/Saida", "/Movimentacoes/Ajuste", "/EntradasFiscais",
        "/EntradasFiscais/Importar", "/EntradasFiscais/Conferir/00000000-0000-0000-0000-000000000000",
        "/SaldosZerados", "/Rupturas", "/Atendimentos"
    ];

    private static readonly string[] AdminPosts =
    [
        "/Produtos/AdicionarNumeracoes/00000000-0000-0000-0000-000000000000",
        "/Produtos/Create", "/Produtos/Edit/00000000-0000-0000-0000-000000000000",
        "/Produtos/ToggleAtivo/00000000-0000-0000-0000-000000000000",
        "/Produtos/Excluir/00000000-0000-0000-0000-000000000000",
        "/Movimentacoes/Entrada", "/Movimentacoes/EntradaLote",
        "/Movimentacoes/Saida", "/Movimentacoes/Ajuste",
        "/EntradasFiscais/Importar", "/EntradasFiscais/Confirmar"
    ];

    private static readonly string[] SellerPosts =
    ["/Estoque/SelecionarSku", "/Estoque/RegistrarDesistiu", "/Estoque/Vender", "/Estoque/RegistrarNaoTinha"];

    [Theory]
    [InlineData("anonymous")]
    [InlineData("vendedor")]
    [InlineData("lojista")]
    public async Task Protected_get_routes_enforce_roles_on_server(string identity)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        if (identity != "anonymous") await Login(client, identity);

        foreach (var route in AdminGets.Append("/Estoque/Consulta"))
        {
            using var response = await client.GetAsync(route);
            var allowed = route.StartsWith("/Estoque/", StringComparison.Ordinal)
                ? identity == "vendedor" : identity == "lojista";
            if (allowed)
                Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NotFound,
                    $"Acesso permitido inesperado em {route}: {response.StatusCode}");
            else
            {
                Assert.Equal(HttpStatusCode.Found, response.StatusCode);
                Assert.StartsWith(identity == "anonymous" ? "/Account/Login?ReturnUrl=" : "/Account/AccessDenied?ReturnUrl=",
                    response.Headers.Location?.PathAndQuery);
            }
        }
    }

    [Theory]
    [InlineData("anonymous")]
    [InlineData("vendedor")]
    [InlineData("lojista")]
    public async Task Unauthorized_posts_are_denied_before_data_changes(string identity)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        if (identity != "anonymous") await Login(client, identity);
        var token = await TokenFrom(client, identity == "vendedor" ? "/Estoque/Consulta" :
            identity == "lojista" ? "/Produtos" : "/Account/Login");
        var before = await Counts(factory);
        var routes = identity == "lojista" ? SellerPosts : AdminPosts;

        foreach (var route in routes)
        {
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            { ["__RequestVerificationToken"] = token });
            using var response = await client.PostAsync(route, content);
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.StartsWith(identity == "anonymous" ? "/Account/Login?ReturnUrl=" : "/Account/AccessDenied?ReturnUrl=",
                response.Headers.Location?.PathAndQuery);
        }
        Assert.Equal(before, await Counts(factory));
    }

    [Theory]
    [InlineData("lojista")]
    [InlineData("vendedor")]
    public async Task Every_authorized_write_rejects_missing_csrf_token_without_data_changes(string identity)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        await Login(client, identity);
        var before = await Counts(factory);
        foreach (var route in identity == "lojista" ? AdminPosts : SellerPosts)
        {
            using var response = await client.PostAsync(route,
                new FormUrlEncodedContent(new Dictionary<string, string>()));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        Assert.Equal(before, await Counts(factory));
    }

    [Theory]
    [InlineData("https://outside.example.test/path")]
    [InlineData("//outside.example.test/path")]
    public async Task External_return_url_is_ignored_after_successful_login(string returnUrl)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        var token = await TokenFrom(client, "/Account/Login");
        using var response = await SubmitLogin(client, "lojista", token, false, returnUrl);
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/Produtos", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Cookie_attributes_and_remember_me_persistence_are_correct(bool remember)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        var token = await TokenFrom(client, "/Account/Login");
        using var response = await SubmitLogin(client, "lojista", token, remember);
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"),
            value => value.StartsWith(".AspNetCore.Cookies=", StringComparison.Ordinal));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(remember, cookie.Contains("expires=", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Produtos")).StatusCode);
    }

    [Fact]
    public async Task Expiration_and_sliding_renewal_use_controlled_clock()
    {
        var clock = new AdjustableClock();
        using var factory = new SquadEstoqueWebApplicationFactory { AuthenticationClock = clock };
        using var client = CreateClient(factory);
        await Login(client, "lojista", remember: true);

        clock.Advance(TimeSpan.FromHours(7));
        using var midpoint = await client.GetAsync("/Produtos");
        Assert.Equal(HttpStatusCode.OK, midpoint.StatusCode);
        Assert.Contains(midpoint.Headers.GetValues("Set-Cookie"),
            value => value.StartsWith(".AspNetCore.Cookies=", StringComparison.Ordinal));

        clock.Advance(TimeSpan.FromHours(7));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Produtos")).StatusCode);

        clock.Advance(TimeSpan.FromHours(13));
        using var expired = await client.GetAsync("/Produtos");
        Assert.Equal(HttpStatusCode.Found, expired.StatusCode);
        Assert.StartsWith("/Account/Login?ReturnUrl=", expired.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task Logout_removes_browser_cookie_but_a_copied_ticket_remains_valid_until_expiration()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        var token = await TokenFrom(client, "/Account/Login");
        using var login = await SubmitLogin(client, "lojista", token, true);
        var oldCookie = Assert.Single(login.Headers.GetValues("Set-Cookie"),
            value => value.StartsWith(".AspNetCore.Cookies=", StringComparison.Ordinal)).Split(';')[0];
        token = await TokenFrom(client, "/Produtos");
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        { ["__RequestVerificationToken"] = token });
        using var logout = await client.PostAsync("/Account/Logout", form);
        Assert.Equal(HttpStatusCode.Found, logout.StatusCode);
        Assert.StartsWith("/Account/Login?ReturnUrl=", (await client.GetAsync("/Produtos")).Headers.Location?.PathAndQuery);

        using var replay = CreateClient(factory, handleCookies: false);
        replay.DefaultRequestHeaders.Add("Cookie", oldCookie);
        using var replayResponse = await replay.GetAsync("/Produtos");
        Assert.Equal(HttpStatusCode.OK, replayResponse.StatusCode);
    }

    private static HttpClient CreateClient(SquadEstoqueWebApplicationFactory factory, bool handleCookies = true) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        { AllowAutoRedirect = false, HandleCookies = handleCookies });

    private static async Task Login(HttpClient client, string identity, bool remember = false)
    {
        var token = await TokenFrom(client, "/Account/Login");
        using var response = await SubmitLogin(client, identity, token, remember);
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
    }

    private static async Task<HttpResponseMessage> SubmitLogin(HttpClient client, string identity,
        string token, bool remember, string? returnUrl = null)
    {
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = identity == "vendedor" ? "vendedor@squad.com" : "lojista@squad.com",
            ["Senha"] = "123", ["LembrarMe"] = remember.ToString().ToLowerInvariant(),
            ["__RequestVerificationToken"] = token
        });
        return await client.PostAsync("/Account/Login" + (returnUrl is null ? "" : "?returnUrl=" + Uri.EscapeDataString(returnUrl)), form);
    }

    private static async Task<string> TokenFrom(HttpClient client, string route)
    {
        using var response = await client.GetAsync(route);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success, $"Token antiforgery ausente em {route}");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static async Task<(int Users, int Products, int Skus, int Movements, int Attendances, int Ruptures, int Invoices, string ProductState, string SkuState)> Counts(
        SquadEstoqueWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var products = await db.Produto.AsNoTracking().OrderBy(p => p.Id)
            .Select(p => new { p.Id, p.Nome, p.Ativo }).ToListAsync();
        var skus = await db.Sku.AsNoTracking().OrderBy(s => s.Id)
            .Select(s => new { s.Id, s.SaldoAtual, s.Ativo }).ToListAsync();
        return (await db.Usuario.CountAsync(), await db.Produto.CountAsync(), await db.Sku.CountAsync(),
            await db.Movimentacao.CountAsync(), await db.Atendimento.CountAsync(),
            await db.Ruptura.CountAsync(), await db.EntradaFiscal.CountAsync(),
            string.Join('|', products), string.Join('|', skus));
    }

    private sealed class AdjustableClock : TimeProvider
    {
        private DateTimeOffset _now = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => _now;
        public void Advance(TimeSpan interval) => _now += interval;
    }
}
