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

/// <summary>
/// S3-QA-009: comprova o filtro e o agrupamento da consulta de saldos zerados
/// sobre um banco SQLite isolado e com dados determinísticos.
/// </summary>
public sealed class SaldosZeradosConsultaTests
{
    private static readonly Guid BotaAlfaId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid BotaAlfaHomonimaId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid TenisBetaId = Guid.Parse("00000000-0000-0000-0000-000000000003");
    private static readonly Guid ProdutoInativoId = Guid.Parse("00000000-0000-0000-0000-000000000004");
    private static readonly Guid TenisGamaId = Guid.Parse("00000000-0000-0000-0000-000000000005");
    private static readonly Guid AgataId = Guid.Parse("00000000-0000-0000-0000-000000000006");
    private static readonly Guid BotaAlfaBrancaId = Guid.Parse("00000000-0000-0000-0000-000000000007");
    private static readonly Guid BotaBetaId = Guid.Parse("00000000-0000-0000-0000-000000000008");
    private static readonly Guid ProdutoSoSkuInativoId = Guid.Parse("00000000-0000-0000-0000-000000000009");

    [Fact]
    public async Task Index_inclui_somente_skus_ativos_com_saldo_exatamente_zero()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        await SeedGradesAsync(factory);

        var resultado = await ConsultarAsync(factory);

        Assert.Equal(6, resultado.Produtos.Count);
        Assert.Contains(resultado.Produtos, produto => produto.Id == BotaAlfaId);
        Assert.Contains(resultado.Produtos, produto => produto.Id == BotaAlfaHomonimaId);
        Assert.Contains(resultado.Produtos, produto => produto.Id == BotaAlfaBrancaId);
        Assert.Contains(resultado.Produtos, produto => produto.Id == BotaBetaId);
        Assert.Contains(resultado.Produtos, produto => produto.Id == TenisGamaId);
        Assert.Contains(resultado.Produtos, produto => produto.Id == AgataId);
        Assert.DoesNotContain(resultado.Produtos, produto => produto.Id == TenisBetaId);
        Assert.DoesNotContain(resultado.Produtos, produto => produto.Id == ProdutoInativoId);
        // Produto ativo cujo único SKU zerado está inativo também fica de fora.
        Assert.DoesNotContain(resultado.Produtos, produto => produto.Id == ProdutoSoSkuInativoId);
    }

    [Fact]
    public async Task Index_agrupa_cada_produto_pelo_id_sem_misturar_homonimos()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        await SeedGradesAsync(factory);

        var resultado = await ConsultarAsync(factory);

        var bota = Assert.Single(resultado.Produtos, produto => produto.Id == BotaAlfaId);
        var homonima = Assert.Single(resultado.Produtos, produto => produto.Id == BotaAlfaHomonimaId);
        var gama = Assert.Single(resultado.Produtos, produto => produto.Id == TenisGamaId);

        Assert.Equal(new[] { "38", "40" }, bota.NumeracoesZeradas.Select(sku => sku.Numeracao));
        Assert.Equal(new[] { "37" }, homonima.NumeracoesZeradas.Select(sku => sku.Numeracao));
        Assert.Equal(new[] { "10", "35/36", "9" }, gama.NumeracoesZeradas.Select(sku => sku.Numeracao));

        // A numeração 41 está zerada, mas o SKU está inativo: não deve aparecer.
        Assert.DoesNotContain(bota.NumeracoesZeradas, sku => sku.Numeracao == "41");
        // A numeração 39 está ativa, mas possui saldo positivo: não deve aparecer.
        Assert.DoesNotContain(bota.NumeracoesZeradas, sku => sku.Numeracao == "39");
    }

    [Fact]
    public async Task Index_ordena_produtos_por_nome_marca_cor_e_id_com_comparacao_ordinal()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        await SeedGradesAsync(factory);

        var resultado = await ConsultarAsync(factory);

        Assert.Equal(
            new[]
            {
                BotaAlfaId, BotaAlfaHomonimaId, BotaAlfaBrancaId, BotaBetaId, TenisGamaId, AgataId
            },
            resultado.Produtos.Select(produto => produto.Id));
        // "Ágata" vem depois de "Tênis Gama" porque a ordenação é ordinal,
        // e não segue a collation cultural que colocaria "Á" antes de "B".
        Assert.Equal("Ágata", resultado.Produtos[^1].Nome);
    }

    [Fact]
    public async Task Index_retorna_estado_vazio_quando_nao_ha_sku_ativo_zerado()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            await LimparCatalogoAsync(context);
            context.Produto.Add(Produto(
                TenisBetaId, "Tênis Beta", "Beta", "Preto", ativo: true,
                ("42", 5, true)));
            context.Produto.Add(Produto(
                ProdutoInativoId, "Produto inativo", "Beta", "Preto", ativo: false,
                ("43", 0, true)));
            await context.SaveChangesAsync();
        }

        var resultado = await ConsultarAsync(factory);
        Assert.Empty(resultado.Produtos);

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        await LoginAsync(client, "lojista@squad.com");
        using var response = await client.GetAsync("/SaldosZerados");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Nenhum produto possui numerações com saldo zerado no momento.", html);
    }

    [Fact]
    public async Task Consulta_nao_cria_ruptura_ou_movimentacao_nem_altera_saldos()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        var skuId = await SeedHistoricoAsync(factory);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        await LoginAsync(client, "lojista@squad.com");

        using var response = await client.GetAsync("/SaldosZerados");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Saldos zerados", html);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var sku = await context.Sku.AsNoTracking().SingleAsync(item => item.Id == skuId);

        Assert.Equal(1, await context.Movimentacao.AsNoTracking().CountAsync());
        Assert.Equal(1, await context.Ruptura.AsNoTracking().CountAsync());
        Assert.Equal(0, sku.SaldoAtual);
        Assert.Equal(new DateTime(2026, 10, 20, 9, 0, 0, DateTimeKind.Utc), sku.UltimaAtualizacaoSaldoEm);
    }

    private static async Task<SaldosZeradosViewModel> ConsultarAsync(
        SquadEstoqueWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();

        var resultado = Assert.IsType<ViewResult>(
            await new SaldosZeradosController(context).Index());

        return Assert.IsType<SaldosZeradosViewModel>(resultado.Model);
    }

    private static async Task SeedGradesAsync(SquadEstoqueWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        await LimparCatalogoAsync(context);

        context.Produto.AddRange(
            Produto(BotaAlfaId, "Bota Ágil", "Alfa", "Azul", ativo: true,
                ("38", 0, true),
                ("40", 0, true),
                ("39", 2, true),
                ("41", 0, false)),
            Produto(BotaAlfaHomonimaId, "Bota Ágil", "Alfa", "Azul", ativo: true,
                ("37", 0, true)),
            Produto(TenisBetaId, "Tênis Beta", "Beta", "Preto", ativo: true,
                ("42", 5, true)),
            Produto(BotaAlfaBrancaId, "Bota Ágil", "Alfa", "Branco", ativo: true,
                ("36", 0, true)),
            Produto(BotaBetaId, "Bota Ágil", "Beta", "Azul", ativo: true,
                ("35", 0, true)),
            Produto(ProdutoInativoId, "Produto inativo", "Beta", "Preto", ativo: false,
                ("43", 0, true)),
            Produto(ProdutoSoSkuInativoId, "Produto só com SKU inativo", "Beta", "Preto", ativo: true,
                ("34", 0, false)),
            Produto(TenisGamaId, "Tênis Gama", "Alfa", "Preto", ativo: true,
                ("9", 0, true),
                ("10", 0, true),
                ("35/36", 0, true)),
            Produto(AgataId, "Ágata", "Alfa", "Preto", ativo: true,
                ("44", 0, true)));

        await context.SaveChangesAsync();
    }

    private static async Task<Guid> SeedHistoricoAsync(SquadEstoqueWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        await LimparCatalogoAsync(context);
        var lojistaId = await context.Usuario
            .Where(usuario => usuario.Email == "lojista@squad.com")
            .Select(usuario => usuario.Id)
            .SingleAsync();

        var produto = Produto(
            Guid.Parse("00000000-0000-0000-0000-0000000000a0"),
            "Tênis com histórico", "Alfa", "Preto", ativo: true,
            ("45", 0, true));
        var skuId = Assert.Single(produto.Skus).Id;

        context.Produto.Add(produto);
        context.Movimentacao.Add(new Movimentacao
        {
            Id = Guid.Parse("00000000-0000-0000-0000-0000000000a2"),
            SkuId = skuId,
            Tipo = TipoMovimentacao.ENTRADA,
            Quantidade = 1,
            UsuarioId = lojistaId,
            CriadoEm = new DateTime(2026, 10, 19, 8, 0, 0, DateTimeKind.Utc)
        });
        context.Ruptura.Add(new Ruptura
        {
            Id = Guid.Parse("00000000-0000-0000-0000-0000000000a3"),
            SkuId = skuId,
            UsuarioId = lojistaId,
            CriadoEm = new DateTime(2026, 10, 19, 8, 5, 0, DateTimeKind.Utc)
        });
        await context.SaveChangesAsync();

        return skuId;
    }

    private static async Task LimparCatalogoAsync(EstoqueContext context)
    {
        context.Sku.RemoveRange(context.Sku);
        context.Produto.RemoveRange(context.Produto);
        await context.SaveChangesAsync();
    }

    private static Produto Produto(
        Guid id,
        string nome,
        string marca,
        string cor,
        bool ativo,
        params (string Numeracao, int Saldo, bool Ativo)[] skus) => new()
    {
        Id = id,
        Nome = nome,
        Marca = marca,
        Categoria = "Calçado",
        Cor = cor,
        Ativo = ativo,
        Skus = skus.Select(sku => new Sku
        {
            Id = Guid.NewGuid(),
            ProdutoId = id,
            Numeracao = sku.Numeracao,
            SaldoAtual = sku.Saldo,
            Ativo = sku.Ativo,
            UltimaAtualizacaoSaldoEm = sku.Saldo == 0
                ? new DateTime(2026, 10, 20, 9, 0, 0, DateTimeKind.Utc)
                : null
        }).ToList()
    };

    private static async Task LoginAsync(HttpClient client, string email)
    {
        using var page = await client.GetAsync("/Account/Login");
        var match = Regex.Match(await page.Content.ReadAsStringAsync(),
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"[^>]*>",
            RegexOptions.IgnoreCase);
        Assert.True(match.Success, "Token antiforgery não encontrado.");

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Senha"] = "123",
            ["__RequestVerificationToken"] = WebUtility.HtmlDecode(match.Groups[1].Value)
        });
        using var response = await client.PostAsync("/Account/Login", form);
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
    }
}
