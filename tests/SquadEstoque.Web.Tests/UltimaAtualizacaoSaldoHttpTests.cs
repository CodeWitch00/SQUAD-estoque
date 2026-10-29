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

public sealed class UltimaAtualizacaoSaldoHttpTests
{
    public static IEnumerable<object[]> Cenarios()
    {
        foreach (var semData in new[] { false, true })
        foreach (var operacao in new[] { "entrada", "venda", "ajuste", "ruptura", "desistiu", "saidaRejeitada", "ajusteIgual", "ajusteSemMotivo" })
            yield return new object[] { operacao, semData };
    }

    [Theory]
    [MemberData(nameof(Cenarios))]
    public async Task Data_persistida_muda_apenas_apos_operacao_confirmada(string operacao, bool semData)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        DateTime? anterior = semData ? null : new DateTime(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        Guid skuId;
        Guid produtoId;
        Dictionary<Guid, Snapshot> antes;
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            var sku = await context.Sku.SingleAsync(s => s.Numeracao == "39");
            sku.UltimaAtualizacaoSaldoEm = anterior;
            skuId = sku.Id;
            produtoId = sku.ProdutoId;
            await context.SaveChangesAsync();
            antes = await SnapshotAsync(context);
        }

        var vendedor = operacao is "venda" or "ruptura" or "desistiu";
        using var loginPage = await client.GetAsync("/Account/Login");
        using var loginForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = vendedor ? "vendedor@squad.com" : "lojista@squad.com",
            ["Senha"] = "123",
            ["__RequestVerificationToken"] = await TokenAsync(loginPage)
        });
        using var login = await client.PostAsync("/Account/Login", loginForm);
        Assert.Equal(HttpStatusCode.Found, login.StatusCode);

        var endpoint = operacao switch
        {
            "entrada" => "/Movimentacoes/Entrada",
            "venda" => "/Estoque/Vender",
            "ruptura" => "/Estoque/RegistrarNaoTinha",
            "desistiu" => "/Estoque/RegistrarDesistiu",
            "saidaRejeitada" => "/Movimentacoes/Saida",
            _ => "/Movimentacoes/Ajuste"
        };
        using var page = await client.GetAsync(vendedor ? "/Estoque/Consulta" : $"{endpoint}?skuId={skuId}");
        var token = await TokenAsync(page);
        var values = new Dictionary<string, string>
        {
            ["skuId"] = skuId.ToString(),
            ["produtoId"] = produtoId.ToString(),
            ["Quantidade"] = operacao == "saidaRejeitada" ? "3" : "2",
            ["NovoSaldoApurado"] = operacao == "ajusteIgual" ? "2" : "5",
            ["Motivo"] = operacao == "ajusteSemMotivo" ? "" : "Contagem física",
            ["__RequestVerificationToken"] = token
        };
        if (operacao == "desistiu")
        {
            using var selecaoForm = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["skuId"] = skuId.ToString(), ["__RequestVerificationToken"] = token
            });
            using var selecao = await client.PostAsync("/Estoque/SelecionarSku", selecaoForm);
            Assert.Equal(HttpStatusCode.OK, selecao.StatusCode);
            using var body = JsonDocument.Parse(await selecao.Content.ReadAsStringAsync());
            values["atendimentoId"] = body.RootElement.GetProperty("atendimentoId").GetGuid().ToString();
        }

        // Also catch accidental stock/date changes caused by consultation or selection.
        using (var scope = factory.Services.CreateScope())
            Assert.Equal(antes.OrderBy(s => s.Key),
                (await SnapshotAsync(scope.ServiceProvider.GetRequiredService<EstoqueContext>())).OrderBy(s => s.Key));

        using var form = new FormUrlEncodedContent(values);
        var inicio = DateTime.UtcNow;
        using var response = await client.PostAsync(endpoint, form);
        var fim = DateTime.UtcNow;
        Assert.Equal(operacao is "entrada" or "ajuste" ? HttpStatusCode.Found : HttpStatusCode.OK, response.StatusCode);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        if (operacao == "saidaRejeitada") Assert.Contains("Saldo insuficiente", html);
        if (operacao == "ajusteIgual") Assert.Contains("Não há alteração de saldo", html);
        if (operacao == "ajusteSemMotivo") Assert.Contains("O motivo do ajuste é obrigatório.", html);
        if (operacao == "venda") Assert.Contains("Venda registrada com sucesso.", html);

        using var verification = factory.Services.CreateScope();
        var persisted = verification.ServiceProvider.GetRequiredService<EstoqueContext>();
        var depois = await SnapshotAsync(persisted);
        Assert.Equal(antes.Keys.OrderBy(id => id), depois.Keys.OrderBy(id => id));
        foreach (var (id, snapshot) in antes.Where(s => s.Key != skuId))
            Assert.Equal(snapshot, depois[id]);

        var altera = operacao is "entrada" or "venda" or "ajuste";
        if (altera)
        {
            Assert.NotNull(depois[skuId].Data);
            Assert.Equal(DateTimeKind.Utc, depois[skuId].Data!.Value.Kind);
            Assert.InRange(depois[skuId].Data!.Value, inicio, fim);
            Assert.NotEqual(anterior, depois[skuId].Data);
            Assert.Equal(operacao == "entrada" ? 4 : operacao == "venda" ? 1 : 5, depois[skuId].Saldo);
            var movimento = Assert.Single(await persisted.Movimentacao.AsNoTracking().ToListAsync());
            Assert.Equal(skuId, movimento.SkuId);
            Assert.Equal(operacao == "entrada" ? 2 : operacao == "venda" ? 1 : 3, movimento.Quantidade);
            Assert.Equal(operacao == "entrada" ? TipoMovimentacao.ENTRADA :
                operacao == "venda" ? TipoMovimentacao.SAIDA : TipoMovimentacao.AJUSTE, movimento.Tipo);
            Assert.InRange(movimento.CriadoEm, inicio, fim);
        }
        else
        {
            Assert.Equal(antes[skuId], depois[skuId]);
            Assert.Empty(await persisted.Movimentacao.AsNoTracking().ToListAsync());
        }

        var rupturas = await persisted.Ruptura.AsNoTracking().ToListAsync();
        if (operacao == "ruptura") Assert.Equal(skuId, Assert.Single(rupturas).SkuId);
        else Assert.Empty(rupturas);
        if (operacao == "desistiu")
        {
            var atendimento = Assert.Single(await persisted.Atendimento.AsNoTracking().ToListAsync());
            Assert.Equal(StatusAtendimento.CONCLUIDO, atendimento.Status);
            Assert.Equal(ResultadoAtendimento.DESISTIU, atendimento.Resultado);
        }
    }

    private sealed record Snapshot(int Saldo, DateTime? Data);

    private static Task<Dictionary<Guid, Snapshot>> SnapshotAsync(EstoqueContext context) =>
        context.Sku.AsNoTracking().ToDictionaryAsync(s => s.Id, s => new Snapshot(s.SaldoAtual, s.UltimaAtualizacaoSaldoEm));

    private static async Task<string> TokenAsync(HttpResponseMessage page)
    {
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        var match = Regex.Match(await page.Content.ReadAsStringAsync(),
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"[^>]*>");
        Assert.True(match.Success, "Token antiforgery não encontrado.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }
}
