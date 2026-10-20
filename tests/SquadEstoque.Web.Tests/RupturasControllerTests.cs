using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Controllers;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class RupturasControllerTests
{
    [Fact]
    public async Task Index_agrupa_rupturas_persistidas_por_sku_com_produto_numeracao_e_ultima_data()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var vendedorId = await context.Usuario
            .Where(usuario => usuario.Email == "vendedor@squad.com")
            .Select(usuario => usuario.Id)
            .SingleAsync();

        var produto = new Produto
        {
            Id = Guid.NewGuid(), Nome = "Bota", Marca = "Marca A", Categoria = "Calçado", Cor = "Azul"
        };
        var primeiroSku = new Sku { Id = Guid.NewGuid(), Numeracao = "38", SaldoAtual = 0 };
        var segundoSku = new Sku { Id = Guid.NewGuid(), Numeracao = "39", SaldoAtual = 2 };
        var semRuptura = new Sku { Id = Guid.NewGuid(), Numeracao = "40", SaldoAtual = 0 };
        produto.Skus.Add(primeiroSku);
        produto.Skus.Add(segundoSku);
        produto.Skus.Add(semRuptura);
        var outroProduto = new Produto
        {
            Id = Guid.NewGuid(), Nome = "Tênis", Marca = "Marca B", Categoria = "Calçado", Cor = "Preto"
        };
        var terceiroSku = new Sku { Id = Guid.NewGuid(), Numeracao = "38", SaldoAtual = 1 };
        outroProduto.Skus.Add(terceiroSku);
        context.Produto.AddRange(produto, outroProduto);

        var primeiraData = new DateTime(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc);
        var ultimaData = primeiraData.AddDays(2);
        context.Ruptura.AddRange(
            NovaRuptura(primeiroSku.Id, vendedorId, primeiraData),
            NovaRuptura(primeiroSku.Id, vendedorId, ultimaData),
            NovaRuptura(segundoSku.Id, vendedorId, primeiraData.AddDays(1)),
            NovaRuptura(terceiroSku.Id, vendedorId, primeiraData.AddHours(1)));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var resultado = Assert.IsType<ViewResult>(await new RupturasController(context).Index());
        var historico = Assert.IsType<RupturasIndexViewModel>(resultado.Model).Rupturas;

        Assert.Equal(3, historico.Count);
        var bota38 = Assert.Single(historico, item => item.Produto == "Bota" && item.Numeracao == "38");
        Assert.Equal("Marca A", bota38.Marca);
        Assert.Equal("Azul", bota38.Cor);
        Assert.Equal(2, bota38.QuantidadeOcorrencias);
        Assert.Equal(ultimaData, bota38.UltimaOcorrencia);

        var bota39 = Assert.Single(historico, item => item.Produto == "Bota" && item.Numeracao == "39");
        Assert.Equal(1, bota39.QuantidadeOcorrencias);
        Assert.Equal(primeiraData.AddDays(1), bota39.UltimaOcorrencia);
        Assert.Equal(1, Assert.Single(historico, item => item.Produto == "Tênis").QuantidadeOcorrencias);
        Assert.DoesNotContain(historico, item => item.Numeracao == "40");
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public async Task Index_sem_rupturas_retorna_historico_vazio_mesmo_com_saldos_zerados()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();

        var resultado = Assert.IsType<ViewResult>(await new RupturasController(context).Index());

        Assert.Empty(Assert.IsType<RupturasIndexViewModel>(resultado.Model).Rupturas);
    }

    [Theory]
    [InlineData("vendedor@squad.com", "/Account/AccessDenied")]
    [InlineData(null, "/Account/Login")]
    public async Task Index_restringe_acesso_ao_lojista(string? email, string destino)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        if (email != null)
        {
            using var pagina = await client.GetAsync("/Account/Login");
            var html = await pagina.Content.ReadAsStringAsync();
            var match = Regex.Match(html,
                "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"[^>]*>",
                RegexOptions.IgnoreCase);
            Assert.True(match.Success);
            using var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = email,
                ["Senha"] = "123",
                ["__RequestVerificationToken"] = WebUtility.HtmlDecode(match.Groups[1].Value)
            });
            using var login = await client.PostAsync("/Account/Login", form);
            Assert.Equal(HttpStatusCode.Found, login.StatusCode);
        }

        using var resposta = await client.GetAsync("/Rupturas");
        Assert.Equal(HttpStatusCode.Found, resposta.StatusCode);
        Assert.StartsWith(destino, resposta.Headers.Location?.PathAndQuery);
    }

    private static Ruptura NovaRuptura(Guid skuId, Guid vendedorId, DateTime data) => new()
    {
        Id = Guid.NewGuid(), SkuId = skuId, UsuarioId = vendedorId, CriadoEm = data
    };
}
