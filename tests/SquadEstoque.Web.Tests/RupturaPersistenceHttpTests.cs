using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class RupturaPersistenceHttpTests
{
    [Fact]
    public async Task Nao_tinha_persists_one_rupture_with_sku_authenticated_seller_and_date()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        var produtoId = Guid.NewGuid();
        var skuId = Guid.NewGuid();
        var vendedorId = Guid.NewGuid();
        var email = $"ruptura-{vendedorId:N}@example.test";

        using (var setupScope = factory.Services.CreateScope())
        {
            var context = setupScope.ServiceProvider.GetRequiredService<EstoqueContext>();
            context.Usuario.Add(new Usuario
            {
                Id = vendedorId,
                Nome = "Vendedor rastreabilidade ruptura",
                Email = email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("123", 12),
                Perfil = PerfilUsuario.VENDEDOR
            });
            var produto = new Produto
            {
                Id = produtoId,
                Nome = "Tênis ruptura rastreável",
                Marca = "Squad",
                Categoria = "Calçado",
                Cor = "Preto",
                Ativo = true
            };
            produto.Skus.Add(new Sku
            {
                Id = skuId,
                ProdutoId = produtoId,
                Numeracao = "42",
                SaldoAtual = 1,
                Ativo = true
            });
            context.Produto.Add(produto);
            await context.SaveChangesAsync();
            Assert.Empty(await context.Ruptura.AsNoTracking().ToListAsync());
        }

        using var loginPage = await client.GetAsync("/Account/Login");
        using var loginForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Senha"] = "123",
            ["__RequestVerificationToken"] = await ExtractTokenAsync(loginPage)
        });
        using var loginResponse = await client.PostAsync("/Account/Login", loginForm);
        Assert.Equal(HttpStatusCode.Found, loginResponse.StatusCode);

        using var consulta = await client.GetAsync("/Estoque/Consulta");
        using var rupturaForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["skuId"] = skuId.ToString(),
            ["__RequestVerificationToken"] = await ExtractTokenAsync(consulta)
        });
        var inicio = DateTime.UtcNow;
        using var response = await client.PostAsync("/Estoque/RegistrarNaoTinha", rupturaForm);
        var fim = DateTime.UtcNow;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var verificationScope = factory.Services.CreateScope();
        var persisted = verificationScope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var ruptura = Assert.Single(await persisted.Ruptura.AsNoTracking().ToListAsync());
        Assert.Equal(skuId, ruptura.SkuId);
        Assert.Equal(vendedorId, ruptura.UsuarioId);
        Assert.InRange(ruptura.CriadoEm, inicio, fim);
    }

    private static async Task<string> ExtractTokenAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(
            html,
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"[^>]*>",
            RegexOptions.IgnoreCase);

        Assert.True(match.Success, "Token antiforgery não encontrado.");
        return match.Groups[1].Value;
    }
}
