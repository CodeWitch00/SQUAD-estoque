using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class DesistiuHttpTests
{
    [Theory]
    [InlineData(false, 0)]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 0)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    public async Task Desistiu_ends_context_without_persistence_and_allows_another_query(bool enviarSku, int saldo)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        var produto = await AddProdutoAsync(factory, saldo);
        var proximoProduto = await AddProdutoAsync(factory, 3);
        var before = await SnapshotAsync(factory);
        await LoginAsync(client, "vendedor@squad.com");
        using var grade = await client.GetAsync($"/Estoque/Consulta?termo={Uri.EscapeDataString(produto.Nome)}&produtoId={produto.Id}");
        var html = WebUtility.HtmlDecode(await grade.Content.ReadAsStringAsync());
        Assert.Contains($"data-sku-id=\"{produto.Skus.Single().Id}\"", html);
        Assert.Contains("data-desistiu-url=\"/Estoque/RegistrarDesistiu\"", html);
        var values = new Dictionary<string, string> { ["__RequestVerificationToken"] = await TokenAsync(grade) };
        // A selected SKU is optional context and must never trigger a stock operation.
        if (enviarSku) values["skuId"] = produto.Skus.Single().Id.ToString();
        string? novaConsultaUrl = null;
        for (var tentativa = 0; tentativa < 2; tentativa++)
        {
            using var form = new FormUrlEncodedContent(values);
            using var response = await client.PostAsync("/Estoque/RegistrarDesistiu", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("desistiu", payload.GetProperty("resultado").GetString());
            Assert.True(payload.GetProperty("atendimentoEncerrado").GetBoolean());
            Assert.Equal("Atendimento encerrado.", payload.GetProperty("mensagem").GetString());
            novaConsultaUrl = payload.GetProperty("novaConsultaUrl").GetString();
            Assert.Equal("/Estoque/Consulta", novaConsultaUrl);
            await AssertUnchangedAsync(factory, before);
        }
        using var novaConsulta = await client.GetAsync(novaConsultaUrl);
        var novaHtml = await novaConsulta.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, novaConsulta.StatusCode);
        Assert.DoesNotContain("id=\"grade\"", novaHtml);
        Assert.DoesNotContain("consulta-atendimento-acoes", novaHtml);
        Assert.Matches("<input(?=[^>]*id=\"Termo\")(?=[^>]*value=\"\")[^>]*>", novaHtml);
        await AssertUnchangedAsync(factory, before);

        using var busca = await client.GetAsync($"{novaConsultaUrl}?termo={Uri.EscapeDataString(proximoProduto.Nome)}");
        Assert.Equal(HttpStatusCode.OK, busca.StatusCode);
        var buscaHtml = WebUtility.HtmlDecode(await busca.Content.ReadAsStringAsync());
        Assert.Contains(proximoProduto.Nome, buscaHtml);
        var resultado = Regex.Match(buscaHtml,
            $"<a\\b[^>]*href=\"([^\"]*produtoId={proximoProduto.Id}[^\"]*)\"");
        Assert.True(resultado.Success, "A próxima busca deve permitir selecionar outro produto.");

        using var outraGrade = await client.GetAsync(resultado.Groups[1].Value);
        Assert.Equal(HttpStatusCode.OK, outraGrade.StatusCode);
        var outraHtml = WebUtility.HtmlDecode(await outraGrade.Content.ReadAsStringAsync());
        Assert.Contains("id=\"grade\"", outraHtml);
        Assert.Contains(proximoProduto.Nome, outraHtml);
        Assert.Contains($"data-sku-id=\"{proximoProduto.Skus.Single().Id}\"", outraHtml);
        Assert.DoesNotContain($"data-sku-id=\"{produto.Skus.Single().Id}\"", outraHtml);
        Assert.Contains("data-resultado=\"desistiu\"", outraHtml);
        await AssertUnchangedAsync(factory, before);
    }

    [Theory]
    [InlineData(null, "/Account/Login", "/Account/Login?ReturnUrl=%2FEstoque%2FRegistrarDesistiu")]
    [InlineData("lojista@squad.com", "/Produtos", "/Account/AccessDenied?ReturnUrl=%2FEstoque%2FRegistrarDesistiu")]
    public async Task Desistiu_requires_authenticated_seller(string? email, string tokenPage, string redirect)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        var before = await SnapshotAsync(factory);
        if (email is not null) await LoginAsync(client, email);
        using var page = await client.GetAsync(tokenPage);
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = await TokenAsync(page)
        });
        using var response = await client.PostAsync("/Estoque/RegistrarDesistiu", form);
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal(redirect, response.Headers.Location?.PathAndQuery);
        await AssertUnchangedAsync(factory, before);
    }

    [Fact]
    public async Task Desistiu_rejects_missing_antiforgery_and_get()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        var before = await SnapshotAsync(factory);
        await LoginAsync(client, "vendedor@squad.com");
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>());
        using var post = await client.PostAsync("/Estoque/RegistrarDesistiu", form);
        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);
        using var get = await client.GetAsync("/Estoque/RegistrarDesistiu");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        await AssertUnchangedAsync(factory, before);
    }

    private static async Task<Produto> AddProdutoAsync(SquadEstoqueWebApplicationFactory factory, int saldo)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var produto = new Produto
        {
            Id = Guid.NewGuid(),
            Nome = $"Tênis desistiu {Guid.NewGuid():N}",
            Marca = "Squad",
            Categoria = "Calçado",
            Cor = "Preto",
            Ativo = true
        };
        produto.Skus.Add(new Sku
        {
            Id = Guid.NewGuid(), ProdutoId = produto.Id, Numeracao = "38",
            SaldoAtual = saldo, Ativo = true
        });
        context.Produto.Add(produto);
        await context.SaveChangesAsync();
        return produto;
    }

    private static HttpClient CreateClient(SquadEstoqueWebApplicationFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });

    private static async Task LoginAsync(HttpClient client, string email)
    {
        using var page = await client.GetAsync("/Account/Login");
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email, ["Senha"] = "123", ["__RequestVerificationToken"] = await TokenAsync(page)
        });
        using var response = await client.PostAsync("/Account/Login", form);
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
    }

    private static async Task<string> TokenAsync(HttpResponseMessage page)
    {
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        var match = Regex.Match(await page.Content.ReadAsStringAsync(),
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"[^>]*>");
        Assert.True(match.Success);
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static async Task<(Dictionary<Guid, int> Saldos, Guid[] Movimentacoes, Guid[] Rupturas)> SnapshotAsync(
        SquadEstoqueWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        return (await context.Sku.AsNoTracking().ToDictionaryAsync(s => s.Id, s => s.SaldoAtual),
            await context.Movimentacao.OrderBy(m => m.Id).Select(m => m.Id).ToArrayAsync(),
            await context.Ruptura.OrderBy(r => r.Id).Select(r => r.Id).ToArrayAsync());
    }

    private static async Task AssertUnchangedAsync(SquadEstoqueWebApplicationFactory factory,
        (Dictionary<Guid, int> Saldos, Guid[] Movimentacoes, Guid[] Rupturas) before)
    {
        var after = await SnapshotAsync(factory);
        Assert.Equal(before.Saldos.OrderBy(s => s.Key), after.Saldos.OrderBy(s => s.Key));
        Assert.Equal(before.Movimentacoes, after.Movimentacoes);
        Assert.Equal(before.Rupturas, after.Rupturas);
    }
}
