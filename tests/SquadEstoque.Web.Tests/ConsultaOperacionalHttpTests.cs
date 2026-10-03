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
        foreach (var (skuId, saldo) in saldosEsperados)
        {
            var card = Regex.Match(visibleHtml,
                $"<li[^>]*data-sku-id=\"{skuId}\"[^>]*>.*?</li>", RegexOptions.Singleline);
            Assert.True(card.Success, $"SKU {skuId} não encontrado na grade.");
            Assert.Contains("class=\"consulta-grade-card\"", card.Value);
            Assert.Contains($"type=\"radio\" name=\"skuSelecionado\" value=\"{skuId}\"", card.Value);
            Assert.DoesNotContain("Vendeu", card.Value);
        }
        Assert.Contains("Resultado do atendimento", visibleHtml);
        Assert.Matches("data-resultado=\"vendeu\"[^>]*disabled", visibleHtml);
        Assert.Contains("data-vender-url=\"/Estoque/Vender\"", visibleHtml);
        Assert.Contains("data-resultado=\"nao-tinha\" disabled", visibleHtml);
        Assert.Contains("data-resultado=\"desistiu\"", visibleHtml);
        Assert.Contains("Selecione a numeração solicitada pelo cliente.", visibleHtml);
        Assert.Contains("Nova consulta sem registrar resultado", visibleHtml);
        Assert.DoesNotContain("/Movimentacoes/Saida", visibleHtml, StringComparison.OrdinalIgnoreCase);
        await AssertSkuBalancesAsync(saldosEsperados);
    }

    [Fact]
    public async Task Nova_consulta_sem_desfecho_retorna_a_busca_inicial_sem_persistir_resultado()
    {
        var produto = await AddProdutoAsync(true, ("38", 2));
        var skuId = produto.Skus.Single().Id;
        using var client = CreateClient();
        await LoginAsVendedorAsync(client);

        Dictionary<Guid, int> saldosAntes;
        int movimentacoesAntes;
        int rupturasAntes;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            saldosAntes = await context.Sku.AsNoTracking().Where(s => s.Id == skuId)
                .ToDictionaryAsync(s => s.Id, s => s.SaldoAtual);
            movimentacoesAntes = await context.Movimentacao.CountAsync();
            rupturasAntes = await context.Ruptura.CountAsync();
        }

        var atendimentoResponse = await client.GetAsync(
            $"/Estoque/Consulta?termo={Uri.EscapeDataString(produto.Nome)}&produtoId={produto.Id}");
        var atendimentoHtml = WebUtility.HtmlDecode(await atendimentoResponse.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, atendimentoResponse.StatusCode);
        Assert.Contains($"data-sku-id=\"{skuId}\"", atendimentoHtml);
        var link = Regex.Match(atendimentoHtml,
            "<a class=\"consulta-nova-consulta\" href=\"([^\"]+)\"[^>]*>Nova consulta sem registrar resultado</a>");
        Assert.True(link.Success, "O atendimento deve oferecer a ação de nova consulta sem desfecho.");

        var novaConsultaResponse = await client.GetAsync(link.Groups[1].Value);
        var novaConsultaHtml = WebUtility.HtmlDecode(await novaConsultaResponse.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, novaConsultaResponse.StatusCode);
        Assert.Contains("Pronto para consultar", novaConsultaHtml);
        Assert.DoesNotContain("id=\"grade\"", novaConsultaHtml);
        Assert.DoesNotContain("consulta-atendimento-acoes", novaConsultaHtml);
        Assert.Matches("<input(?=[^>]*id=\"Termo\")(?=[^>]*value=\"\")[^>]*>", novaConsultaHtml);

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            var saldosDepois = await context.Sku.AsNoTracking().Where(s => s.Id == skuId)
                .ToDictionaryAsync(s => s.Id, s => s.SaldoAtual);
            Assert.Equal(saldosAntes, saldosDepois);
            Assert.Equal(movimentacoesAntes, await context.Movimentacao.CountAsync());
            Assert.Equal(rupturasAntes, await context.Ruptura.CountAsync());
        }
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

    [Theory]
    [InlineData("Tenis")]
    [InlineData("tesnis")]
    [InlineData("Squad")]
    public async Task Consulta_accepts_accent_free_and_small_typo_searches(string termo)
    {
        using var client = CreateClient();
        await LoginAsVendedorAsync(client);

        var response = await client.GetAsync($"/Estoque/Consulta?termo={Uri.EscapeDataString(termo)}");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Tênis Runner", html);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Desistiu_retorna_a_consulta_preserva_estoque_e_permite_proximo_atendimento(int saldo)
    {
        var produto = await AddProdutoAsync(true, ("38", saldo));
        var proximoProduto = await AddProdutoAsync(true, ("40", 3));
        using var client = CreateClient();
        await LoginAsVendedorAsync(client);
        var antes = await ReadEstoqueSnapshotAsync();

        var atendimento = await client.GetAsync(
            $"/Estoque/Consulta?termo={Uri.EscapeDataString(produto.Nome)}&produtoId={produto.Id}");
        Assert.Equal(HttpStatusCode.OK, atendimento.StatusCode);
        var html = WebUtility.HtmlDecode(await atendimento.Content.ReadAsStringAsync());
        Assert.Contains($"data-sku-id=\"{produto.Skus.Single().Id}\"", html);
        var desistiu = Regex.Match(html,
            "<button\\b(?=[^>]*data-resultado=\"desistiu\")[^>]*>\\s*Desistiu\\s*</button>");
        Assert.True(desistiu.Success, "O atendimento deve oferecer o botão Desistiu.");
        Assert.Contains("type=\"button\"", desistiu.Value);
        Assert.DoesNotContain("disabled", desistiu.Value);

        // TestServer não executa JavaScript. Verificamos o contrato do script servido
        // e seguimos seu destino por HTTP; isto não substitui um teste de clique/DOM.
        var scriptTag = Regex.Match(html, "<script\\b[^>]*src=\"([^\"]*/js/consulta\\.js[^\"]*)\"");
        Assert.True(scriptTag.Success, "A página deve carregar o script do atendimento.");
        var scriptResponse = await client.GetAsync(scriptTag.Groups[1].Value);
        Assert.Equal(HttpStatusCode.OK, scriptResponse.StatusCode);
        var script = await scriptResponse.Content.ReadAsStringAsync();
        var navegacao = Regex.Match(script,
            "if\\s*\\(tipo\\s*===\\s*'desistiu'\\)\\s*\\{\\s*window\\.location\\.assign\\('([^']+)'\\);\\s*return;\\s*\\}");
        Assert.True(navegacao.Success,
            "Desistiu deve navegar e encerrar seu ramo sem registrar venda ou ruptura.");
        var destino = navegacao.Groups[1].Value;
        Assert.Equal("/Estoque/Consulta", destino);

        var consulta = await client.GetAsync(destino);
        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
        var consultaHtml = WebUtility.HtmlDecode(await consulta.Content.ReadAsStringAsync());
        Assert.Contains("Pronto para consultar", consultaHtml);
        Assert.DoesNotContain("id=\"grade\"", consultaHtml);
        Assert.DoesNotContain("consulta-atendimento-acoes", consultaHtml);
        Assert.Matches("<input(?=[^>]*id=\"Termo\")(?=[^>]*value=\"\")[^>]*>", consultaHtml);
        await AssertEstoqueUnchangedAsync(antes);

        var busca = await client.GetAsync($"{destino}?termo={Uri.EscapeDataString(proximoProduto.Nome)}");
        Assert.Equal(HttpStatusCode.OK, busca.StatusCode);
        var buscaHtml = WebUtility.HtmlDecode(await busca.Content.ReadAsStringAsync());
        Assert.Contains(proximoProduto.Nome, buscaHtml);
        var resultado = Regex.Match(buscaHtml,
            $"<a\\b[^>]*href=\"([^\"]*produtoId={proximoProduto.Id}[^\"]*)\"");
        Assert.True(resultado.Success, "A próxima busca deve permitir selecionar outro produto.");

        var proximoAtendimento = await client.GetAsync(resultado.Groups[1].Value);
        Assert.Equal(HttpStatusCode.OK, proximoAtendimento.StatusCode);
        var proximoHtml = WebUtility.HtmlDecode(await proximoAtendimento.Content.ReadAsStringAsync());
        Assert.Contains($"{proximoProduto.Nome} selecionado.", proximoHtml);
        Assert.Contains($"data-sku-id=\"{proximoProduto.Skus.Single().Id}\"", proximoHtml);
        Assert.Contains("data-resultado=\"desistiu\"", proximoHtml);
        await AssertEstoqueUnchangedAsync(antes);
    }

    private async Task<(Dictionary<Guid, int> Saldos, Guid[] Movimentacoes, Guid[] Rupturas)> ReadEstoqueSnapshotAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        return (
            await context.Sku.AsNoTracking().ToDictionaryAsync(s => s.Id, s => s.SaldoAtual),
            await context.Movimentacao.OrderBy(m => m.Id).Select(m => m.Id).ToArrayAsync(),
            await context.Ruptura.OrderBy(r => r.Id).Select(r => r.Id).ToArrayAsync());
    }

    private async Task AssertEstoqueUnchangedAsync(
        (Dictionary<Guid, int> Saldos, Guid[] Movimentacoes, Guid[] Rupturas) antes)
    {
        var depois = await ReadEstoqueSnapshotAsync();
        Assert.Equal(antes.Saldos, depois.Saldos);
        Assert.Equal(antes.Movimentacoes, depois.Movimentacoes);
        Assert.Equal(antes.Rupturas, depois.Rupturas);
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
