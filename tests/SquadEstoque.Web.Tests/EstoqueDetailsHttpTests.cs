using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class EstoqueDetailsHttpTests : IClassFixture<SquadEstoqueWebApplicationFactory>
{
    private readonly SquadEstoqueWebApplicationFactory _factory;

    public EstoqueDetailsHttpTests(SquadEstoqueWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Detalhes_renders_complete_sorted_grade_with_visible_stock_states()
    {
        var produto = await AddProdutoAsync(true, ("40", 5), ("37", 0), ("39", 2), ("38", 1));
        using var client = CreateClient();
        await LoginAsVendedorAsync(client);

        var response = await client.GetAsync($"/Estoque/Detalhes/{produto.Id}");
        var html = await response.Content.ReadAsStringAsync();
        var visibleHtml = WebUtility.HtmlDecode(html);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Grade de numerações", visibleHtml);
        Assert.Contains("Consulta operacional", visibleHtml);
        Assert.Contains("Nº 37", visibleHtml);
        Assert.Contains("Nº 38", visibleHtml);
        Assert.Contains("Nº 39", visibleHtml);
        Assert.Contains("Nº 40", visibleHtml);
        Assert.True(IndexOf(visibleHtml, "Nº 37") < IndexOf(visibleHtml, "Nº 38"));
        Assert.True(IndexOf(visibleHtml, "Nº 38") < IndexOf(visibleHtml, "Nº 39"));
        Assert.True(IndexOf(visibleHtml, "Nº 39") < IndexOf(visibleHtml, "Nº 40"));
        Assert.Contains("<strong>0</strong> pares", visibleHtml);
        Assert.Contains("<strong>1</strong> par", visibleHtml);
        Assert.Contains("<strong>2</strong> pares", visibleHtml);
        Assert.Contains("<strong>5</strong> pares", visibleHtml);
        Assert.Contains(">Indisponível<", visibleHtml);
        Assert.Contains(">Último par<", visibleHtml);
        Assert.Contains(">Disponível<", visibleHtml);
    }

    [Fact]
    public async Task Detalhes_returns_not_found_for_inactive_product()
    {
        var produto = await AddProdutoAsync(false, ("38", 2));
        using var client = CreateClient();
        await LoginAsVendedorAsync(client);

        var response = await client.GetAsync($"/Estoque/Detalhes/{produto.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Detalhes_returns_not_found_for_nonexistent_product()
    {
        using var client = CreateClient();
        await LoginAsVendedorAsync(client);

        var response = await client.GetAsync($"/Estoque/Detalhes/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true
    });

    private async Task<Produto> AddProdutoAsync(bool ativo, params (string numeracao, int saldo)[] grade)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var produto = new Produto
        {
            Id = Guid.NewGuid(),
            Nome = $"Tênis consulta {Guid.NewGuid():N}",
            Marca = "Squad",
            Categoria = "Calçado",
            Cor = "Preto",
            Ativo = ativo
        };

        foreach (var (numeracao, saldo) in grade)
        {
            produto.Skus.Add(new Sku
            {
                Id = Guid.NewGuid(),
                ProdutoId = produto.Id,
                Numeracao = numeracao,
                SaldoAtual = saldo,
                Ativo = true
            });
        }

        context.Produto.Add(produto);
        await context.SaveChangesAsync();
        return produto;
    }

    private static async Task LoginAsVendedorAsync(HttpClient client)
    {
        var loginPage = await client.GetAsync("/Account/Login");
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiforgeryToken(html);
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "vendedor@squad.com",
            ["Senha"] = "123",
            ["__RequestVerificationToken"] = token
        });

        var response = await client.PostAsync("/Account/Login", content);
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
    }

    private static string ExtractAntiforgeryToken(string html)
    {
        var match = Regex.Match(html, "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"[^>]*>", RegexOptions.IgnoreCase);

        Assert.True(match.Success, "Token antiforgery não encontrado no formulário de login.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static int IndexOf(string value, string text)
    {
        var index = value.IndexOf(text, StringComparison.Ordinal);
        Assert.True(index >= 0, $"Texto '{text}' não foi encontrado.");
        return index;
    }
}
