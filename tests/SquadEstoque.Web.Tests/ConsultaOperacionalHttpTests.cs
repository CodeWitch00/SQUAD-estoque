using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class ConsultaOperacionalHttpTests : IClassFixture<SquadEstoqueWebApplicationFactory>
{
    private readonly SquadEstoqueWebApplicationFactory _factory;

    public ConsultaOperacionalHttpTests(SquadEstoqueWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Consulta_renders_selected_product_with_complete_sorted_grade_and_visible_stock_states()
    {
        var produto = await AddProdutoAsync(true, ("40", 5), ("37", 0), ("39", 2), ("38", 1));
        var saldosEsperados = produto.Skus.ToDictionary(s => s.Id, s => s.SaldoAtual);
        using var client = CreateClient();
        await LoginAsVendedorAsync(client);

        var response = await client.GetAsync($"/Estoque/Consulta?termo={Uri.EscapeDataString(produto.Nome)}&produtoId={produto.Id}");
        var html = await response.Content.ReadAsStringAsync();
        var visibleHtml = WebUtility.HtmlDecode(html);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Grade completa de numerações", visibleHtml);
        Assert.Contains("Grade disponível", visibleHtml);
        Assert.Matches(NumberPattern("37"), visibleHtml);
        Assert.Matches(NumberPattern("38"), visibleHtml);
        Assert.Matches(NumberPattern("39"), visibleHtml);
        Assert.Matches(NumberPattern("40"), visibleHtml);
        Assert.True(IndexOfPattern(visibleHtml, NumberPattern("37")) < IndexOfPattern(visibleHtml, NumberPattern("38")));
        Assert.True(IndexOfPattern(visibleHtml, NumberPattern("38")) < IndexOfPattern(visibleHtml, NumberPattern("39")));
        Assert.True(IndexOfPattern(visibleHtml, NumberPattern("39")) < IndexOfPattern(visibleHtml, NumberPattern("40")));
        Assert.Matches(@"0\s*pares", visibleHtml);
        Assert.Matches(@"1\s*par", visibleHtml);
        Assert.Matches(@"2\s*pares", visibleHtml);
        Assert.Matches(@"5\s*pares", visibleHtml);
        Assert.Contains("Indisponível", visibleHtml);
        Assert.Contains("Último par", visibleHtml);
        Assert.Contains("Disponível", visibleHtml);
        Assert.DoesNotContain("Venda", visibleHtml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ruptura", visibleHtml, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/Movimentacoes/Saida", visibleHtml, StringComparison.OrdinalIgnoreCase);
        await AssertSkuBalancesAsync(saldosEsperados);
    }

    [Fact]
    public async Task Consulta_does_not_render_inactive_product()
    {
        var produto = await AddProdutoAsync(false, ("38", 2));
        using var client = CreateClient();
        await LoginAsVendedorAsync(client);

        var response = await client.GetAsync($"/Estoque/Consulta?termo={Uri.EscapeDataString(produto.Nome)}&produtoId={produto.Id}");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Produto não encontrado.", WebUtility.HtmlDecode(html));
    }

    [Fact]
    public async Task Consulta_does_not_select_nonexistent_product()
    {
        using var client = CreateClient();
        await LoginAsVendedorAsync(client);

        var response = await client.GetAsync($"/Estoque/Consulta?termo=produto-inexistente&produtoId={Guid.NewGuid()}");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Produto não encontrado.", WebUtility.HtmlDecode(html));
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

    private async Task AssertSkuBalancesAsync(IReadOnlyDictionary<Guid, int> saldosEsperados)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var saldosAtuais = await context.Sku
            .AsNoTracking()
            .Where(s => saldosEsperados.Keys.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.SaldoAtual);

        Assert.Equal(saldosEsperados, saldosAtuais);
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

    private static int IndexOfPattern(string value, string pattern)
    {
        var match = Regex.Match(value, pattern);
        Assert.True(match.Success, $"Padrão '{pattern}' não foi encontrado.");
        return match.Index;
    }

    private static string NumberPattern(string numeracao) => $@"Nº(?:\s|<[^>]+>)*{Regex.Escape(numeracao)}";
}
