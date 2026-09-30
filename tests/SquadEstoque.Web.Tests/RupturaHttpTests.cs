using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class RupturaHttpTests
{
    [Fact]
    public async Task Nao_tinha_preserves_positive_balance_and_persists_rupture_without_movement()
    {
        // An isolated relational database keeps this scenario independent of sales.
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        var skuId = Guid.NewGuid();
        const int saldoInicial = 5;
        Guid vendedorId;

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            var produto = new Produto
            {
                Id = Guid.NewGuid(), Nome = "Tênis ruptura QA", Marca = "Squad",
                Categoria = "Calçado", Cor = "Azul", Ativo = true
            };
            produto.Skus.Add(new Sku
            {
                Id = skuId, ProdutoId = produto.Id, Numeracao = "40",
                SaldoAtual = saldoInicial, Ativo = true
            });
            context.Produto.Add(produto);
            await context.SaveChangesAsync();
            var vendedor = await context.Usuario.SingleAsync(u => u.Email == "vendedor@squad.com");
            Assert.Equal(PerfilUsuario.VENDEDOR, vendedor.Perfil);
            vendedorId = vendedor.Id;
            Assert.Equal(saldoInicial, await context.Sku.AsNoTracking()
                .Where(s => s.Id == skuId).Select(s => s.SaldoAtual).SingleAsync());
            Assert.Empty(await context.Movimentacao.AsNoTracking().ToListAsync());
            Assert.Empty(await context.Ruptura.AsNoTracking().ToListAsync());
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
        using var rupturaForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["skuId"] = skuId.ToString(),
            ["__RequestVerificationToken"] = await TokenAsync(consulta)
        });
        var inicio = DateTime.UtcNow;
        using var response = await client.PostAsync("/Estoque/RegistrarNaoTinha", rupturaForm);
        var fim = DateTime.UtcNow;
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Reload from the database in a new scope, never from the setup tracker.
        using var verificationScope = factory.Services.CreateScope();
        var persisted = verificationScope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var skuRecarregado = await persisted.Sku.AsNoTracking().SingleAsync(s => s.Id == skuId);
        Assert.Equal(saldoInicial, skuRecarregado.SaldoAtual);
        Assert.Empty(await persisted.Movimentacao.AsNoTracking().ToListAsync());
        var ruptura = Assert.Single(await persisted.Ruptura.AsNoTracking().ToListAsync());
        Assert.NotEqual(Guid.Empty, ruptura.Id);
        Assert.Equal(skuId, ruptura.SkuId);
        Assert.Equal(vendedorId, ruptura.UsuarioId);
        Assert.InRange(ruptura.CriadoEm, inicio, fim);
    }

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
