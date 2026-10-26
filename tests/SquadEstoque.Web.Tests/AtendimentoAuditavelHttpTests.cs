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

public sealed class AtendimentoAuditavelHttpTests
{
    [Fact]
    public async Task Abrir_modelo_nao_cria_pendencia_antes_de_escolher_numero()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        await LoginAsync(client, "vendedor@squad.com");
        var produto = await ProdutoAsync(factory);

        using var grade = await client.GetAsync(
            $"/Estoque/Consulta?termo={Uri.EscapeDataString(produto.Nome)}&produtoId={produto.Id}");
        var gradeHtml = WebUtility.HtmlDecode(await grade.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, grade.StatusCode);
        Assert.Contains("Qual número?", gradeHtml);
        Assert.Contains(">Disponíveis<", gradeHtml);
        Assert.Contains(">Indisponíveis<", gradeHtml);
        Assert.True(gradeHtml.IndexOf(">Disponíveis<", StringComparison.Ordinal) <
            gradeHtml.IndexOf(">Indisponíveis<", StringComparison.Ordinal));
        using var busca = await client.GetAsync("/Estoque/Consulta");
        var buscaHtml = WebUtility.HtmlDecode(await busca.Content.ReadAsStringAsync());
        Assert.DoesNotContain("Atendimentos pendentes", buscaHtml);
        Assert.Contains("data-atendimento-id=\"\"", gradeHtml);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        Assert.Empty(await context.Atendimento.Where(a => a.ProdutoId == produto.Id).ToListAsync());
    }

    [Fact]
    public async Task Selecionar_numeracao_salva_sku_e_saldo_e_restaura_a_escolha_ao_retomar()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        await LoginAsync(client, "vendedor@squad.com");
        var produto = await ProdutoAsync(factory);
        var sku = produto.Skus.Single(s => s.SaldoAtual == 2);
        var outroSku = produto.Skus.First(s => s.Id != sku.Id);
        using var grade = await client.GetAsync(
            $"/Estoque/Consulta?termo={Uri.EscapeDataString(produto.Nome)}&produtoId={produto.Id}");
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["atendimentoId"] = string.Empty,
            ["skuId"] = sku.Id.ToString(),
            ["__RequestVerificationToken"] = await TokenAsync(grade)
        });

        using var selecao = await client.PostAsync("/Estoque/SelecionarSku", form);
        Assert.Equal(HttpStatusCode.OK, selecao.StatusCode);
        var payload = await selecao.Content.ReadFromJsonAsync<JsonElement>();
        var atendimentoId = payload.GetProperty("atendimentoId").GetGuid();

        using var busca = await client.GetAsync("/Estoque/Consulta");
        var buscaHtml = WebUtility.HtmlDecode(await busca.Content.ReadAsStringAsync());
        Assert.Contains($"Numeração solicitada: {sku.Numeracao}", buscaHtml);

        using var retomada = await client.GetAsync($"/Estoque/Consulta?atendimentoId={atendimentoId}");
        var retomadaHtml = WebUtility.HtmlDecode(await retomada.Content.ReadAsStringAsync());
        Assert.Matches(
            $"<input(?=[^>]*value=\"{sku.Id}\")(?=[^>]*checked=\"checked\")[^>]*>",
            retomadaHtml);
        Assert.DoesNotContain($"data-sku-id=\"{outroSku.Id}\"", retomadaHtml);
        Assert.Contains("Número selecionado", retomadaHtml);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var atendimento = await context.Atendimento.AsNoTracking().SingleAsync(a => a.Id == atendimentoId);
        Assert.Equal(StatusAtendimento.PENDENTE, atendimento.Status);
        Assert.Equal(sku.Id, atendimento.SkuId);
        Assert.Equal(2, atendimento.SaldoConsultado);
        Assert.Empty(await context.Movimentacao.Where(m => m.AtendimentoId == atendimentoId).ToListAsync());
        Assert.Empty(await context.Ruptura.Where(r => r.AtendimentoId == atendimentoId).ToListAsync());
    }

    [Fact]
    public async Task Selecionar_numeracao_indisponivel_nao_a_associa_a_pendencia()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        await LoginAsync(client, "vendedor@squad.com");
        var produto = await ProdutoAsync(factory);
        var skuIndisponivel = produto.Skus.Single(s => s.SaldoAtual == 0);
        using var grade = await client.GetAsync(
            $"/Estoque/Consulta?termo={Uri.EscapeDataString(produto.Nome)}&produtoId={produto.Id}");
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["atendimentoId"] = string.Empty,
            ["skuId"] = skuIndisponivel.Id.ToString(),
            ["__RequestVerificationToken"] = await TokenAsync(grade)
        });

        using var selecao = await client.PostAsync("/Estoque/SelecionarSku", form);
        var payload = await selecao.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.OK, selecao.StatusCode);
        Assert.False(payload.GetProperty("persistido").GetBoolean());

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        Assert.Empty(await context.Atendimento.Where(a => a.ProdutoId == produto.Id).ToListAsync());
    }

    [Fact]
    public async Task Venda_conclui_atendimento_uma_vez_e_vincula_movimentacao()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        await LoginAsync(client, "vendedor@squad.com");
        var produto = await ProdutoAsync(factory);
        var sku = produto.Skus.Single(s => s.SaldoAtual == 2);
        using var grade = await client.GetAsync(
            $"/Estoque/Consulta?termo={Uri.EscapeDataString(produto.Nome)}&produtoId={produto.Id}");
        var atendimentoId = await SelecionarSkuAsync(client, grade, sku.Id);
        var token = await TokenAsync(grade);
        var values = new Dictionary<string, string>
        {
            ["skuId"] = sku.Id.ToString(), ["produtoId"] = produto.Id.ToString(),
            ["atendimentoId"] = atendimentoId.ToString(), ["__RequestVerificationToken"] = token
        };

        using var form = new FormUrlEncodedContent(values);
        using var response = await client.PostAsync("/Estoque/Vender", form);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var repeticaoForm = new FormUrlEncodedContent(values);
        using var repeticao = await client.PostAsync("/Estoque/Vender", repeticaoForm);
        Assert.Equal(HttpStatusCode.BadRequest, repeticao.StatusCode);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var atendimento = await context.Atendimento.AsNoTracking().SingleAsync(a => a.Id == atendimentoId);
        Assert.Equal(StatusAtendimento.CONCLUIDO, atendimento.Status);
        Assert.Equal(ResultadoAtendimento.VENDEU, atendimento.Resultado);
        Assert.Equal(sku.Id, atendimento.SkuId);
        Assert.Equal(2, atendimento.SaldoConsultado);
        var movimento = await context.Movimentacao.AsNoTracking().SingleAsync(m => m.AtendimentoId == atendimentoId);
        Assert.Equal(TipoMovimentacao.SAIDA, movimento.Tipo);
        Assert.Equal(1, movimento.Quantidade);
        Assert.Equal(1, await context.Sku.Where(s => s.Id == sku.Id).Select(s => s.SaldoAtual).SingleAsync());
    }

    [Fact]
    public async Task Nao_tinha_conclui_atendimento_e_vincula_ruptura_sem_alterar_saldo()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        await LoginAsync(client, "vendedor@squad.com");
        var produto = await ProdutoAsync(factory);
        var sku = produto.Skus.Single(s => s.SaldoAtual == 1);
        using var grade = await client.GetAsync(
            $"/Estoque/Consulta?termo={Uri.EscapeDataString(produto.Nome)}&produtoId={produto.Id}");
        var atendimentoId = await SelecionarSkuAsync(client, grade, sku.Id);
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["skuId"] = sku.Id.ToString(), ["produtoId"] = produto.Id.ToString(),
            ["atendimentoId"] = atendimentoId.ToString(), ["__RequestVerificationToken"] = await TokenAsync(grade)
        });

        using var response = await client.PostAsync("/Estoque/RegistrarNaoTinha", form);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var atendimento = await context.Atendimento.AsNoTracking().SingleAsync(a => a.Id == atendimentoId);
        Assert.Equal(ResultadoAtendimento.NAO_TINHA, atendimento.Resultado);
        Assert.Equal(1, atendimento.SaldoConsultado);
        Assert.Equal(1, await context.Sku.Where(s => s.Id == sku.Id).Select(s => s.SaldoAtual).SingleAsync());
        Assert.NotNull(await context.Ruptura.AsNoTracking().SingleOrDefaultAsync(r => r.AtendimentoId == atendimentoId));
    }

    [Fact]
    public async Task Modelo_totalmente_indisponivel_nao_cria_pendencia_mas_permite_registrar_demanda()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        await LoginAsync(client, "vendedor@squad.com");
        var produto = await ProdutoAsync(factory);

        using (var preparationScope = factory.Services.CreateScope())
        {
            var preparation = preparationScope.ServiceProvider.GetRequiredService<EstoqueContext>();
            await preparation.Sku.Where(s => s.ProdutoId == produto.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.SaldoAtual, 0));
        }

        using var grade = await client.GetAsync(
            $"/Estoque/Consulta?termo={Uri.EscapeDataString(produto.Nome)}&produtoId={produto.Id}");
        var gradeHtml = WebUtility.HtmlDecode(await grade.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, grade.StatusCode);
        Assert.Contains("Modelo sem saldo disponível", gradeHtml);
        Assert.DoesNotContain("Registrar desistência", gradeHtml);

        using (var verificationScope = factory.Services.CreateScope())
        {
            var verification = verificationScope.ServiceProvider.GetRequiredService<EstoqueContext>();
            Assert.Empty(await verification.Atendimento.Where(a => a.ProdutoId == produto.Id).ToListAsync());
        }

        var sku = produto.Skus.First();
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["skuId"] = sku.Id.ToString(), ["produtoId"] = produto.Id.ToString(),
            ["atendimentoId"] = string.Empty, ["__RequestVerificationToken"] = await TokenAsync(grade)
        });
        using var response = await client.PostAsync("/Estoque/RegistrarNaoTinha", form);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var finalScope = factory.Services.CreateScope();
        var context = finalScope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var atendimento = await context.Atendimento.AsNoTracking()
            .Where(a => a.ProdutoId == produto.Id && a.SkuId == sku.Id)
            .OrderByDescending(a => a.CriadoEm).FirstAsync();
        Assert.Equal(StatusAtendimento.CONCLUIDO, atendimento.Status);
        Assert.Equal(ResultadoAtendimento.NAO_TINHA, atendimento.Resultado);
        Assert.Equal(sku.Id, atendimento.SkuId);
        Assert.Equal(0, atendimento.SaldoConsultado);
        Assert.NotNull(await context.Ruptura.AsNoTracking().SingleOrDefaultAsync(r => r.AtendimentoId == atendimento.Id));
    }

    [Fact]
    public async Task Outro_vendedor_nao_pode_concluir_atendimento_alheio()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var dono = CreateClient(factory);
        await LoginAsync(dono, "vendedor@squad.com");
        var produto = await ProdutoAsync(factory);
        using var grade = await dono.GetAsync(
            $"/Estoque/Consulta?termo={Uri.EscapeDataString(produto.Nome)}&produtoId={produto.Id}");
        var atendimentoId = await SelecionarSkuAsync(dono, grade, produto.Skus.First(sku => sku.SaldoAtual > 0).Id);

        const string emailOutro = "outro.vendedor@squad.com";
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            context.Usuario.Add(new Usuario
            {
                Id = Guid.NewGuid(), Nome = "Outro vendedor", Email = emailOutro,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("123", 12), Perfil = PerfilUsuario.VENDEDOR
            });
            await context.SaveChangesAsync();
        }

        using var outro = CreateClient(factory);
        await LoginAsync(outro, emailOutro);
        using var paginaOutro = await outro.GetAsync("/Estoque/Consulta");
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["atendimentoId"] = atendimentoId.ToString(),
            ["__RequestVerificationToken"] = await TokenAsync(paginaOutro)
        });
        using var response = await outro.PostAsync("/Estoque/RegistrarDesistiu", form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var verificationScope = factory.Services.CreateScope();
        var verification = verificationScope.ServiceProvider.GetRequiredService<EstoqueContext>();
        Assert.Equal(StatusAtendimento.PENDENTE,
            await verification.Atendimento.Where(a => a.Id == atendimentoId).Select(a => a.Status).SingleAsync());
    }

    [Fact]
    public async Task Auditoria_e_exclusiva_do_lojista_e_exibe_alertas()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var vendedor = CreateClient(factory);
        await LoginAsync(vendedor, "vendedor@squad.com");
        var produto = await ProdutoAsync(factory);
        using var grade = await vendedor.GetAsync(
            $"/Estoque/Consulta?termo={Uri.EscapeDataString(produto.Nome)}&produtoId={produto.Id}");
        _ = await SelecionarSkuAsync(vendedor, grade, produto.Skus.First(sku => sku.SaldoAtual > 0).Id);

        using var negado = await vendedor.GetAsync("/Atendimentos");
        Assert.Equal(HttpStatusCode.Found, negado.StatusCode);
        Assert.StartsWith("/Account/AccessDenied", negado.Headers.Location?.PathAndQuery);

        using var lojista = CreateClient(factory);
        await LoginAsync(lojista, "lojista@squad.com");
        using var response = await lojista.GetAsync("/Atendimentos");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Atendimentos", html);
        Assert.Contains(produto.Nome, html);
        Assert.Contains("Pendente", html);
    }

    private static async Task<Guid> SelecionarSkuAsync(HttpClient client, HttpResponseMessage grade, Guid skuId)
    {
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["atendimentoId"] = string.Empty,
            ["skuId"] = skuId.ToString(),
            ["__RequestVerificationToken"] = await TokenAsync(grade)
        });
        using var response = await client.PostAsync("/Estoque/SelecionarSku", form);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(payload.GetProperty("persistido").GetBoolean());
        return payload.GetProperty("atendimentoId").GetGuid();
    }

    private static async Task<Produto> ProdutoAsync(SquadEstoqueWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        return await context.Produto.AsNoTracking().Include(p => p.Skus)
            .SingleAsync(p => p.Id == Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
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
        var html = await page.Content.ReadAsStringAsync();
        var match = Regex.Match(html,
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"[^>]*>");
        Assert.True(match.Success);
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }
}
