using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class VendaRapidaHttpTests
{
    [Fact]
    public async Task Vendedor_sells_exactly_one_pair_without_changing_any_other_sku()
    {
        // A factory per test owns fresh SQLite in-memory databases, including the seed.
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        var produtoId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var skuId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        const int saldoInicial = 5;
        Guid vendedorId;
        Dictionary<Guid, SkuSnapshot> antes;

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            var produto = new Produto
            {
                Id = produtoId, Nome = "Tênis venda QA", Marca = "Squad",
                Categoria = "Calçado", Cor = "Azul", Ativo = true
            };
            produto.Skus.Add(new Sku
            {
                Id = skuId, ProdutoId = produtoId, Numeracao = "40", SaldoAtual = saldoInicial
            });
            produto.Skus.Add(new Sku
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                ProdutoId = produtoId, Numeracao = "41", SaldoAtual = 7
            });
            context.Produto.Add(produto);
            await context.SaveChangesAsync();
            vendedorId = await context.Usuario.Where(u => u.Email == "vendedor@squad.com")
                .Select(u => u.Id).SingleAsync();
            antes = await SnapshotAsync(context);
            Assert.Empty(await context.Movimentacao.ToListAsync());
        }

        using var loginPage = await client.GetAsync("/Account/Login");
        using var loginForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "vendedor@squad.com", ["Senha"] = "123",
            ["__RequestVerificationToken"] = await TokenAsync(loginPage)
        });
        using var loginResponse = await client.PostAsync("/Account/Login", loginForm);
        Assert.Equal(HttpStatusCode.Found, loginResponse.StatusCode);
        Assert.Equal("/Estoque/Consulta", loginResponse.Headers.Location?.OriginalString);

        using var consulta = await client.GetAsync("/Estoque/Consulta");
        using var vendaForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["skuId"] = skuId.ToString(),
            ["__RequestVerificationToken"] = await TokenAsync(consulta)
        });
        using var response = await client.PostAsync("/Estoque/Vender", vendaForm);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Venda registrada com sucesso.", body.RootElement.GetProperty("mensagem").GetString());
        Assert.Equal(skuId, body.RootElement.GetProperty("skuId").GetGuid());
        Assert.Equal(saldoInicial - 1, body.RootElement.GetProperty("saldoAtual").GetInt32());

        // Read persisted state in a new scope, never from the seeding change tracker.
        using var verificationScope = factory.Services.CreateScope();
        var persisted = verificationScope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var depois = await SnapshotAsync(persisted);
        Assert.Equal(antes.Keys.OrderBy(id => id), depois.Keys.OrderBy(id => id));
        foreach (var (id, sku) in antes)
        {
            var esperado = id == skuId ? sku with { SaldoAtual = sku.SaldoAtual - 1 } : sku;
            Assert.Equal(esperado, depois[id]);
        }

        var movimento = Assert.Single(await persisted.Movimentacao.AsNoTracking().ToListAsync());
        Assert.Equal(skuId, movimento.SkuId);
        Assert.Equal(vendedorId, movimento.UsuarioId);
        Assert.Equal(TipoMovimentacao.SAIDA, movimento.Tipo);
        Assert.Equal(1, movimento.Quantidade);
    }

    private static async Task<Dictionary<Guid, SkuSnapshot>> SnapshotAsync(EstoqueContext context) =>
        await context.Sku.AsNoTracking().ToDictionaryAsync(s => s.Id,
            s => new SkuSnapshot(s.ProdutoId, s.Numeracao, s.SaldoAtual, s.Ativo));

    private sealed record SkuSnapshot(Guid ProdutoId, string Numeracao, int SaldoAtual, bool Ativo);

    private static async Task<string> TokenAsync(HttpResponseMessage page)
    {
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        var match = Regex.Match(await page.Content.ReadAsStringAsync(),
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"[^>]*>",
            RegexOptions.IgnoreCase);
        Assert.True(match.Success, "Token antiforgery não encontrado.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }
}
