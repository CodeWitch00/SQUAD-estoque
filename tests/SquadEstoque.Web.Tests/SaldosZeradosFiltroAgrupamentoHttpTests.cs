using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

/// <summary>
/// S3-QA-009 — Consulta de saldos zerados: filtro, agrupamento, ordenação,
/// estado vazio e ausência de efeitos colaterais.
/// Regra de filtro conforme docs/03-modelagem/modelo_fisico.md (seção 5.3):
/// WHERE s.saldo_atual = 0 AND s.ativo = 1 AND p.ativo = 1.
/// </summary>
public sealed class SaldosZeradosFiltroAgrupamentoHttpTests
{
    [Fact]
    public async Task Consulta_lista_somente_zerados_ativos_de_produtos_ativos()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        var massa = await AddMassaFiltroAsync(factory);
        await LoginAsync(client, "lojista@squad.com");

        using var response = await client.GetAsync("/SaldosZerados");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Produto ativo com SKU zerado ativo é listado.
        Assert.Contains(massa.ProdutoAtivo, html);
        Assert.Contains(">47</span>", html);

        // SKU ativo com saldo positivo não entra no resultado.
        Assert.DoesNotContain(">48</span>", html);

        // SKU zerado mas inativo não entra no resultado.
        Assert.DoesNotContain(">49</span>", html);

        // Produto inativo com SKU zerado ativo não entra no resultado.
        Assert.DoesNotContain(massa.ProdutoInativo, html);
        Assert.DoesNotContain(">50</span>", html);
    }

    [Fact]
    public async Task Agrupamento_exibe_produto_uma_vez_com_somente_as_numeracoes_zeradas()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        var nome = $"Produto agrupado {Guid.NewGuid():N}";
        await AddProdutoAsync(factory, nome, ("40", 0), ("38", 0), ("39", 0), ("41", 2));
        await LoginAsync(client, "lojista@squad.com");

        using var response = await client.GetAsync("/SaldosZerados");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // O produto é agrupado em um único cartão, não um cartão por numeração.
        Assert.Single(Regex.Matches(html, "aria-label=\"" + Regex.Escape(nome) + "\""));
        Assert.Contains(">38</span>", html);
        Assert.Contains(">39</span>", html);
        Assert.Contains(">40</span>", html);

        // A numeração com saldo positivo fica de fora do agrupamento.
        Assert.DoesNotContain(">41</span>", html);
    }

    [Fact]
    public async Task Ordenacao_segue_ordem_textual_de_nome_e_das_numeracoes()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);

        // "Aaa" e "Zzz" cercam o produto seed "Tênis Runner" na ordem ordinal.
        var primeiro = $"Aaa S3QA009 {Guid.NewGuid():N}";
        var ultimo = $"Zzz S3QA009 {Guid.NewGuid():N}";

        // "10", "40" e "9" provam ordenação textual ordinal (e não numérica).
        await AddProdutoAsync(factory, primeiro, ("10", 0), ("40", 0), ("9", 0));
        await AddProdutoAsync(factory, ultimo, ("77", 0));
        await LoginAsync(client, "lojista@squad.com");

        using var response = await client.GetAsync("/SaldosZerados");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var posicaoPrimeiro = html.IndexOf(primeiro, StringComparison.Ordinal);
        var posicaoSeed = html.IndexOf("Tênis Runner", StringComparison.Ordinal);
        var posicaoUltimo = html.IndexOf(ultimo, StringComparison.Ordinal);
        Assert.True(posicaoPrimeiro >= 0, "Produto ordenado primeiro não foi renderizado.");
        Assert.True(posicaoSeed >= 0, "Produto seed não foi renderizado.");
        Assert.True(posicaoUltimo >= 0, "Produto ordenado por último não foi renderizado.");
        Assert.True(posicaoPrimeiro < posicaoSeed,
            "Ordenação por nome deve posicionar 'Aaa' antes de 'Tênis Runner'.");
        Assert.True(posicaoSeed < posicaoUltimo,
            "Ordenação por nome deve posicionar 'Tênis Runner' antes de 'Zzz'.");

        // Numerações em ordem textual ordinal: "10" < "40" < "9".
        var posicao10 = html.IndexOf(">10</span>", StringComparison.Ordinal);
        var posicao40 = html.IndexOf(">40</span>", StringComparison.Ordinal);
        var posicao9 = html.IndexOf(">9</span>", StringComparison.Ordinal);
        Assert.True(posicao10 >= 0 && posicao40 >= 0 && posicao9 >= 0,
            "Numerações zeradas devem ser renderizadas.");
        Assert.True(posicao10 < posicao40 && posicao40 < posicao9,
            "Numerações devem seguir ordem textual ordinal (10, 40, 9).");
    }

    [Fact]
    public async Task Estado_vazio_quando_os_unicos_zerados_pertencem_a_produto_inativo()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);

        // Remove os zerados ativos do seed e deixa um único zerado em produto inativo.
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            var skusAtivos = await context.Sku.Where(s => s.Ativo).ToListAsync();
            foreach (var sku in skusAtivos)
                sku.SaldoAtual = 1;

            context.Produto.Add(new Produto
            {
                Id = Guid.NewGuid(),
                Nome = $"Produto inativo {Guid.NewGuid():N}",
                Marca = "Marca Teste",
                Categoria = "Calçado",
                Cor = "Azul",
                Ativo = false,
                Skus = new List<Sku>
                {
                    new() { Id = Guid.NewGuid(), Numeracao = "50", SaldoAtual = 0, Ativo = true }
                }
            });
            await context.SaveChangesAsync();
        }
        await LoginAsync(client, "lojista@squad.com");

        using var response = await client.GetAsync("/SaldosZerados");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(
            "Nenhum produto possui numerações com saldo zerado no momento.", html);
        Assert.DoesNotContain(">37</span>", html);
        Assert.DoesNotContain(">50</span>", html);
    }

    [Fact]
    public async Task Consulta_nao_cria_ruptura_movimentacao_nem_altera_saldos()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        await AddProdutoAsync(factory, $"Produto somente leitura {Guid.NewGuid():N}", ("42", 0));
        await LoginAsync(client, "lojista@squad.com");

        Dictionary<Guid, int> saldosAntes;
        int movimentacoesAntes;
        int rupturasAntes;
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            saldosAntes = await context.Sku.AsNoTracking()
                .ToDictionaryAsync(s => s.Id, s => s.SaldoAtual);
            movimentacoesAntes = await context.Movimentacao.CountAsync();
            rupturasAntes = await context.Ruptura.CountAsync();
        }

        using var response = await client.GetAsync("/SaldosZerados");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            var saldosDepois = await context.Sku.AsNoTracking()
                .ToDictionaryAsync(s => s.Id, s => s.SaldoAtual);
            Assert.Equal(saldosAntes, saldosDepois);
            Assert.Equal(movimentacoesAntes, await context.Movimentacao.CountAsync());
            Assert.Equal(rupturasAntes, await context.Ruptura.CountAsync());
        }
    }

    private static HttpClient CreateClient(SquadEstoqueWebApplicationFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

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

    private sealed record MassaFiltro(string ProdutoAtivo, string ProdutoInativo);

    private static async Task<MassaFiltro> AddMassaFiltroAsync(SquadEstoqueWebApplicationFactory factory)
    {
        var produtoAtivo = $"Produto ativo {Guid.NewGuid():N}";
        var produtoInativo = $"Produto inativo {Guid.NewGuid():N}";

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();

        context.Produto.Add(new Produto
        {
            Id = Guid.NewGuid(),
            Nome = produtoAtivo,
            Marca = "Marca Teste",
            Categoria = "Calçado",
            Cor = "Azul",
            Ativo = true,
            Skus = new List<Sku>
            {
                // Zerado ativo: deve aparecer.
                new() { Id = Guid.NewGuid(), Numeracao = "47", SaldoAtual = 0, Ativo = true },
                // Saldo positivo: não deve aparecer.
                new() { Id = Guid.NewGuid(), Numeracao = "48", SaldoAtual = 2, Ativo = true },
                // Zerado inativo: não deve aparecer.
                new() { Id = Guid.NewGuid(), Numeracao = "49", SaldoAtual = 0, Ativo = false }
            }
        });

        context.Produto.Add(new Produto
        {
            Id = Guid.NewGuid(),
            Nome = produtoInativo,
            Marca = "Marca Teste",
            Categoria = "Calçado",
            Cor = "Azul",
            Ativo = false,
            Skus = new List<Sku>
            {
                // SKU zerado ativo de produto inativo: não deve aparecer.
                new() { Id = Guid.NewGuid(), Numeracao = "50", SaldoAtual = 0, Ativo = true }
            }
        });

        await context.SaveChangesAsync();
        return new MassaFiltro(produtoAtivo, produtoInativo);
    }

    private static async Task AddProdutoAsync(
        SquadEstoqueWebApplicationFactory factory,
        string nome,
        params (string Numeracao, int Saldo)[] skus)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();

        var produto = new Produto
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Marca = "Marca Teste",
            Categoria = "Calçado",
            Cor = "Azul",
            Ativo = true
        };
        foreach (var (numeracao, saldo) in skus)
        {
            produto.Skus.Add(new Sku
            {
                Id = Guid.NewGuid(),
                Numeracao = numeracao,
                SaldoAtual = saldo,
                Ativo = true
            });
        }

        context.Produto.Add(produto);
        await context.SaveChangesAsync();
    }
}
