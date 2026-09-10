using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class AdministrativeHttpTests : IClassFixture<SquadEstoqueWebApplicationFactory>
{
    private readonly SquadEstoqueWebApplicationFactory _factory;

    public AdministrativeHttpTests(SquadEstoqueWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Lojista_creates_complete_product_and_grade_via_http()
    {
        using var client = CreateClient();
        await LoginAsync(client, "lojista@squad.com");
        var createPage = await client.GetAsync("/Produtos/Create");
        var token = await ExtractAntiforgeryTokenAsync(createPage);
        var marker = Guid.NewGuid().ToString("N");

        using var content = Form(token,
            ("Nome", $"Tênis HTTP {marker}"),
            ("Marca", "Marca HTTP"),
            ("Categoria", "Esportivo"),
            ("Cor", "Azul"),
            ("NumeracoesGrade", "37, 38; 39 40"));

        var response = await client.PostAsync("/Produtos/Create", content);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/Produtos", response.Headers.Location?.OriginalString);
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var product = await context.Produto.Include(p => p.Skus)
            .SingleAsync(p => p.Nome == $"Tênis HTTP {marker}");
        Assert.True(product.Ativo);
        Assert.Equal(new[] { "37", "38", "39", "40" },
            product.Skus.OrderBy(s => s.Numeracao).Select(s => s.Numeracao));
        Assert.All(product.Skus, sku =>
        {
            Assert.True(sku.Ativo);
            Assert.Equal(0, sku.SaldoAtual);
        });
    }

    [Fact]
    public async Task Lojista_edits_and_deactivates_product_via_http()
    {
        var product = await AddProductAsync();
        using var client = CreateClient();
        await LoginAsync(client, "lojista@squad.com");
        var editPage = await client.GetAsync($"/Produtos/Edit/{product.Id}");
        var editToken = await ExtractAntiforgeryTokenAsync(editPage);

        using var editContent = Form(editToken,
            ("Id", product.Id.ToString()),
            ("Nome", "Produto editado"),
            ("Marca", "Marca editada"),
            ("Categoria", "Categoria editada"),
            ("Cor", "Verde"));
        var editResponse = await client.PostAsync($"/Produtos/Edit/{product.Id}", editContent);

        var indexPage = await client.GetAsync("/Produtos");
        var toggleToken = await ExtractAntiforgeryTokenAsync(indexPage);
        using var toggleContent = Form(toggleToken);
        var toggleResponse = await client.PostAsync($"/Produtos/ToggleAtivo/{product.Id}", toggleContent);

        Assert.Equal(HttpStatusCode.Found, editResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Found, toggleResponse.StatusCode);
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var persisted = await context.Produto.SingleAsync(p => p.Id == product.Id);
        Assert.Equal("Produto editado", persisted.Nome);
        Assert.Equal("Marca editada", persisted.Marca);
        Assert.Equal("Categoria editada", persisted.Categoria);
        Assert.Equal("Verde", persisted.Cor);
        Assert.False(persisted.Ativo);
    }

    [Theory]
    [InlineData("/Produtos/Create")]
    [InlineData("/Produtos/Edit/00000000-0000-0000-0000-000000000001")]
    [InlineData("/Produtos/ToggleAtivo/00000000-0000-0000-0000-000000000001")]
    [InlineData("/Movimentacoes/Entrada")]
    [InlineData("/Movimentacoes/Ajuste")]
    public async Task Vendedor_cannot_execute_administrative_post_endpoints(string route)
    {
        using var client = CreateClient();
        await LoginAsync(client, "vendedor@squad.com");

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>());
        var response = await client.PostAsync(route, content);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Account/AccessDenied", response.Headers.Location?.PathAndQuery);
    }

    [Theory]
    [InlineData("/Produtos/Create")]
    [InlineData("/Produtos/Edit/00000000-0000-0000-0000-000000000001")]
    [InlineData("/Produtos/ToggleAtivo/00000000-0000-0000-0000-000000000001")]
    [InlineData("/Movimentacoes/Entrada")]
    [InlineData("/Movimentacoes/Saida")]
    [InlineData("/Movimentacoes/Ajuste")]
    public async Task Anonymous_user_cannot_execute_protected_post_endpoints(string route)
    {
        using var client = CreateClient();
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>());

        var response = await client.PostAsync(route, content);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Account/Login", response.Headers.Location?.PathAndQuery);
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    private async Task<Produto> AddProductAsync()
    {
        var product = new Produto
        {
            Id = Guid.NewGuid(),
            Nome = $"Produto {Guid.NewGuid():N}",
            Marca = "Marca",
            Categoria = "Categoria",
            Cor = "Preto",
            Ativo = true
        };
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        context.Produto.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    private static FormUrlEncodedContent Form(string token, params (string Key, string Value)[] fields)
    {
        var values = fields.ToDictionary(field => field.Key, field => field.Value);
        values["__RequestVerificationToken"] = token;
        return new FormUrlEncodedContent(values);
    }

    private static async Task LoginAsync(HttpClient client, string email)
    {
        var loginPage = await client.GetAsync("/Account/Login");
        var token = await ExtractAntiforgeryTokenAsync(loginPage);
        using var content = Form(token, ("Email", email), ("Senha", "123"));
        var response = await client.PostAsync("/Account/Login", content);
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
    }

    private static async Task<string> ExtractAntiforgeryTokenAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(
            html,
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"[^>]*>",
            RegexOptions.IgnoreCase);
        Assert.True(match.Success, "Token antiforgery não encontrado no formulário.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }
}
