using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
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
        Assert.Contains("produtoId=aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", html);
    }

    [Fact]
    public async Task Consulta_with_less_than_two_characters_does_not_return_results()
    {
        using var client = CreateClient();
        await LoginAsync(client, "vendedor@squad.com");

        var response = await client.GetAsync("/Estoque/Consulta?termo=T");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Digite pelo menos 2 caracteres para buscar.", html);
        Assert.DoesNotContain("Tênis Runner", html);
    }

    [Fact]
    public async Task Vendedor_can_post_venda_for_sku_and_records_authenticated_user()
    {
        using var client = CreateClient();
        await LoginAsync(client, "vendedor@squad.com");
        var (skuId, usuarioId) = await AddSkuAsync(3);
        var token = await ExtractAntiforgeryTokenAsync(client);

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["SkuId"] = skuId.ToString(),
            ["__RequestVerificationToken"] = token
        });

        var response = await client.PostAsync("/Estoque/Vender", form);
        var body = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Venda registrada com sucesso", body);
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        Assert.Equal(2, (await context.Sku.FindAsync(skuId))!.SaldoAtual);
        var movement = await context.Movimentacao.SingleAsync(m => m.SkuId == skuId);
        Assert.Equal(1, movement.Quantidade);
        Assert.Equal(TipoMovimentacao.SAIDA, movement.Tipo);
        Assert.Equal(usuarioId, movement.UsuarioId);
    }

    [Fact]
    public async Task Vender_rejects_invalid_sku_without_changing_stock()
    {
        using var client = CreateClient();
        await LoginAsync(client, "vendedor@squad.com");
        var token = await ExtractAntiforgeryTokenAsync(client);

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["SkuId"] = Guid.NewGuid().ToString(),
            ["__RequestVerificationToken"] = token
        });

        var response = await client.PostAsync("/Estoque/Vender", form);
        var body = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("não está disponível", body);
    }

    [Fact]
    public async Task Vender_rejects_missing_antiforgery_token()
    {
        using var client = CreateClient();
        await LoginAsync(client, "vendedor@squad.com");

        var response = await client.PostAsync("/Estoque/Vender", new FormUrlEncodedContent(
            new Dictionary<string, string> { ["SkuId"] = Guid.NewGuid().ToString() }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("lojista@squad.com", HttpStatusCode.Found)]
    [InlineData(null, HttpStatusCode.Found)]
    public async Task Vender_requires_authenticated_vendedor(string? email, HttpStatusCode expectedStatus)
    {
        using var client = CreateClient();
        if (email is not null)
        {
            await LoginAsync(client, email);
        }

        var response = await client.PostAsync("/Estoque/Vender", new FormUrlEncodedContent(
            new Dictionary<string, string> { ["SkuId"] = Guid.NewGuid().ToString() }));

        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
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

        var response = await client.GetAsync("/Estoque/Consulta?termo=Runner&produtoId=aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("37", html);
        Assert.Contains("0 pares", html);
        Assert.Contains("Indisponível", html);
        Assert.Contains("38", html);
        Assert.Contains("1 par", html);
        Assert.Contains("Último par", html);
        Assert.Contains("39", html);
        Assert.Contains("2 pares", html);
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
        var expectedLocation = email.StartsWith("lojista@", StringComparison.Ordinal)
            ? "/Produtos"
            : "/Estoque/Consulta";

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal(expectedLocation, response.Headers.Location?.OriginalString);
    }

    private async Task<string> ExtractAntiforgeryTokenAsync(HttpClient client)
    {
        var response = await client.GetAsync("/Estoque/Consulta");
        return ExtractAntiforgeryToken(await response.Content.ReadAsStringAsync());
    }

    private async Task<(Guid SkuId, Guid UsuarioId)> AddSkuAsync(int saldo)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var produto = new Produto
        {
            Id = Guid.NewGuid(),
            Nome = $"Produto venda {Guid.NewGuid():N}",
            Marca = "Teste",
            Categoria = "Calçado",
            Cor = "Preto",
            Ativo = true
        };
        var sku = new Sku
        {
            Id = Guid.NewGuid(),
            ProdutoId = produto.Id,
            Numeracao = "42",
            SaldoAtual = saldo,
            Ativo = true
        };
        context.AddRange(produto, sku);
        await context.SaveChangesAsync();
        var usuarioId = await context.Usuario
            .Where(usuario => usuario.Email == "vendedor@squad.com")
            .Select(usuario => usuario.Id)
            .SingleAsync();
        return (sku.Id, usuarioId);
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
