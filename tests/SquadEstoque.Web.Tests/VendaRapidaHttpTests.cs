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
    public async Task Completed_sale_persists_one_traceable_exit_for_authenticated_seller()
    {
        // Each factory owns a separate relational database; no other test supplies data.
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        var produtoId = Guid.NewGuid();
        var skuId = Guid.NewGuid();
        var vendedorId = Guid.NewGuid();
        var email = $"venda-{vendedorId:N}@example.test";

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            // A distinct seller catches accidental attribution to a seeded/default user.
            context.Usuario.Add(new Usuario
            {
                Id = vendedorId, Nome = "Vendedor rastreabilidade QA", Email = email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("123", 12),
                Perfil = PerfilUsuario.VENDEDOR
            });
            var produto = new Produto
            {
                Id = produtoId, Nome = "Tênis rastreabilidade QA", Marca = "Squad",
                Categoria = "Calçado", Cor = "Azul", Ativo = true
            };
            produto.Skus.Add(new Sku
            {
                Id = skuId, ProdutoId = produtoId, Numeracao = "40",
                SaldoAtual = 2, Ativo = true
            });
            context.Produto.Add(produto);
            await context.SaveChangesAsync();
            Assert.Empty(await context.Movimentacao.AsNoTracking().ToListAsync());
        }

        using var loginPage = await client.GetAsync("/Account/Login");
        using var loginForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email, ["Senha"] = "123",
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
        var inicio = DateTime.UtcNow;
        using var response = await client.PostAsync("/Estoque/Vender", vendaForm);
        var fim = DateTime.UtcNow;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Venda registrada com sucesso.", body.RootElement.GetProperty("mensagem").GetString());

        // Query the entire table in a new context: duplicates for any SKU must fail.
        using var verificationScope = factory.Services.CreateScope();
        var persisted = verificationScope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var movimento = Assert.Single(await persisted.Movimentacao.AsNoTracking().ToListAsync());
        Assert.NotEqual(Guid.Empty, movimento.Id);
        Assert.Equal(TipoMovimentacao.SAIDA, movimento.Tipo);
        Assert.Equal(1, movimento.Quantidade);
        Assert.Equal(skuId, movimento.SkuId);
        Assert.Equal(vendedorId, movimento.UsuarioId);
        Assert.InRange(movimento.CriadoEm, inicio, fim);
    }

    [Fact]
    public async Task Vendedor_cannot_sell_zero_stock_and_creates_no_movement()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        var produtoId = Guid.NewGuid();
        var skuId = Guid.NewGuid();
        Dictionary<Guid, SkuSnapshot> antes;

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            var produto = new Produto
            {
                Id = produtoId, Nome = "Tênis sem saldo QA", Marca = "Squad",
                Categoria = "Calçado", Cor = "Azul", Ativo = true
            };
            produto.Skus.Add(new Sku
            {
                Id = skuId, ProdutoId = produtoId, Numeracao = "40",
                SaldoAtual = 0, Ativo = true
            });
            context.Produto.Add(produto);
            await context.SaveChangesAsync();
            var vendedor = await context.Usuario.SingleAsync(u => u.Email == "vendedor@squad.com");
            Assert.Equal(PerfilUsuario.VENDEDOR, vendedor.Perfil);
            antes = await SnapshotAsync(context);
            Assert.Equal(0, antes[skuId].SaldoAtual);
            Assert.Empty(await context.Movimentacao.AsNoTracking().ToListAsync());
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

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Saldo insuficiente para saída. Saldo disponível: 0 par(es).",
            body.RootElement.GetProperty("mensagem").GetString());

        // A new context verifies committed state after the rejected HTTP request.
        using var verificationScope = factory.Services.CreateScope();
        var persisted = verificationScope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var depois = await SnapshotAsync(persisted);
        Assert.Equal(0, depois[skuId].SaldoAtual);
        Assert.Equal(antes.Keys.OrderBy(id => id), depois.Keys.OrderBy(id => id));
        foreach (var (id, sku) in antes)
            Assert.Equal(sku, depois[id]);
        Assert.Empty(await persisted.Movimentacao.AsNoTracking().ToListAsync());
    }

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
