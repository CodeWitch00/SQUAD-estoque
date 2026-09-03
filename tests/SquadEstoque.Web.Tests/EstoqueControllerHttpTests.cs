using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class EstoqueControllerHttpTests : IClassFixture<SquadEstoqueWebApplicationFactory>
{
    private readonly SquadEstoqueWebApplicationFactory _factory;

    public EstoqueControllerHttpTests(SquadEstoqueWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Consulta_without_authentication_redirects_to_login()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/Estoque/Consulta");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/Account/Login?ReturnUrl=%2FEstoque%2FConsulta", response.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task Lojista_cannot_access_consulta_operacional()
    {
        using var client = CreateClient();
        await LoginAsync(client, "lojista@squad.com");

        var response = await client.GetAsync("/Estoque/Consulta");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal(
            "/Account/AccessDenied?ReturnUrl=%2FEstoque%2FConsulta",
            response.Headers.Location?.PathAndQuery);
    }

    [Theory]
    [InlineData("Runner")]
    [InlineData("Squad")]
    [InlineData("Calçado")]
    [InlineData("Preto")]
    public async Task Vendedor_can_search_active_products_by_supported_terms(string termo)
    {
        using var client = CreateClient();
        await LoginAsync(client, "vendedor@squad.com");

        var response = await client.GetAsync($"/Estoque/Consulta?termo={Uri.EscapeDataString(termo)}");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Tênis Runner", html);
        Assert.Contains("/Estoque/Detalhes/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", html);
    }

    [Fact]
    public async Task Consulta_with_less_than_two_characters_does_not_return_results()
    {
        using var client = CreateClient();
        await LoginAsync(client, "vendedor@squad.com");

        var response = await client.GetAsync("/Estoque/Consulta?termo=T");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Informe ao menos 2 caracteres para consultar.", html);
        Assert.DoesNotContain("Tênis Runner", html);
    }

    [Fact]
    public async Task Consulta_without_results_shows_not_found_message()
    {
        using var client = CreateClient();
        await LoginAsync(client, "vendedor@squad.com");

        var response = await client.GetAsync("/Estoque/Consulta?termo=inexistente");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Produto não encontrado.", html);
    }

    [Fact]
    public async Task Detalhes_shows_complete_grade_with_visual_states()
    {
        using var client = CreateClient();
        await LoginAsync(client, "vendedor@squad.com");

        var response = await client.GetAsync("/Estoque/Detalhes/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Nº 37", html);
        Assert.Contains("Indisponível", html);
        Assert.Contains("Nº 38", html);
        Assert.Contains("Último par", html);
        Assert.Contains("Nº 39", html);
        Assert.Contains("Disponível", html);
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
        Assert.Equal("/", response.Headers.Location?.OriginalString);
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
