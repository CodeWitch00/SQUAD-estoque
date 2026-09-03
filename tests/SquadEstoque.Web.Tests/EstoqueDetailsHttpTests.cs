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
    public async Task Consulta_renders_complete_sorted_grade_with_visible_stock_states()
    {
        var produto = await AddProdutoAsync(true, ("40", 5), ("37", 0), ("39", 2), ("38", 1));
        using var client = CreateClient();
        await LoginAsVendedorAsync(client);

        var response = await client.GetAsync($"/Estoque/Consulta?termo={Uri.EscapeDataString(produto.Nome)}&produtoId={produto.Id}");
        var html = await response.Content.ReadAsStringAsync();
        var visibleHtml = WebUtility.HtmlDecode(html);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Grade completa de numerações", visibleHtml);
        Assert.Contains($"{produto.Nome} selecionado.", visibleHtml);
        Assert.Contains("Tamanho 37, saldo 0, Indisponível", visibleHtml);
        Assert.Contains("Tamanho 38, saldo 1, Último par", visibleHtml);
        Assert.Contains("Tamanho 39, saldo 2, Disponível", visibleHtml);
        Assert.Contains("Tamanho 40, saldo 5, Disponível", visibleHtml);
        Assert.True(IndexOf(visibleHtml, "Tamanho 37") < IndexOf(visibleHtml, "Tamanho 38"));
        Assert.True(IndexOf(visibleHtml, "Tamanho 38") < IndexOf(visibleHtml, "Tamanho 39"));
        Assert.True(IndexOf(visibleHtml, "Tamanho 39") < IndexOf(visibleHtml, "Tamanho 40"));
    }

    [Fact]
    public async Task Consulta_does_not_return_inactive_product()
    {
        var produto = await AddProdutoAsync(false, ("38", 2));
        using var client = CreateClient();
        await LoginAsVendedorAsync(client);

        var response = await client.GetAsync($"/Estoque/Consulta?termo={Uri.EscapeDataString(produto.Nome)}&produtoId={produto.Id}");
        var visibleHtml = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Produto não encontrado.", visibleHtml);
        Assert.DoesNotContain("Grade completa de numerações", visibleHtml);
        Assert.DoesNotContain("selecionado.", visibleHtml);
    }

    [Fact]
    public async Task Consulta_shows_not_found_for_nonexistent_product()
    {
        using var client = CreateClient();
        await LoginAsVendedorAsync(client);

        var termo = $"inexistente-{Guid.NewGuid():N}";
        var response = await client.GetAsync($"/Estoque/Consulta?termo={termo}&produtoId={Guid.NewGuid()}");
        var visibleHtml = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Produto não encontrado.", visibleHtml);
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
