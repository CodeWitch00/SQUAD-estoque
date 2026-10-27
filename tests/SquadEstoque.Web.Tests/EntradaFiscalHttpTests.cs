using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class EntradaFiscalHttpTests
{
    private const string Chave = "35261012345678000123550010000012341000012345";

    [Fact]
    public async Task Entrada_fiscal_e_exclusiva_do_lojista()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        await LoginAsync(client, "vendedor@squad.com");

        using var response = await client.GetAsync("/EntradasFiscais/Importar");
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith("/Account/AccessDenied", response.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task Xml_cria_rascunho_e_confirmacao_cadastra_produto_sku_movimento_e_vinculo()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        await LoginAsync(client);

        using var importPage = await client.GetAsync("/EntradasFiscais/Importar");
        using var importForm = FormularioXml(await TokenAsync(importPage));
        using var importResponse = await client.PostAsync("/EntradasFiscais/Importar", importForm);
        Assert.Equal(HttpStatusCode.Found, importResponse.StatusCode);

        Guid entradaId;
        Guid itemId;
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            var entrada = await context.EntradaFiscal.Include(e => e.Itens).SingleAsync(e => e.ChaveAcesso == Chave);
            entradaId = entrada.Id;
            itemId = entrada.Itens.Single().Id;
            Assert.Equal(StatusEntradaFiscal.RASCUNHO, entrada.Status);
            Assert.Empty(await context.Movimentacao.ToListAsync());
        }

        using var reviewPage = await client.GetAsync($"/EntradasFiscais/Conferir/{entradaId}");
        var reviewHtml = WebUtility.HtmlDecode(await reviewPage.Content.ReadAsStringAsync());
        Assert.Contains("TÊNIS XML PRETO", reviewHtml);
        Assert.Contains("O estoque não será alterado", WebUtility.HtmlDecode(await importPage.Content.ReadAsStringAsync()));
        using var confirmForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["EntradaId"] = entradaId.ToString(),
            ["Itens[0].ItemId"] = itemId.ToString(),
            ["Itens[0].NumeroItem"] = "1",
            ["Itens[0].CodigoFornecedor"] = "REF-PTO-38",
            ["Itens[0].Descricao"] = "TÊNIS XML PRETO 38",
            ["Itens[0].Quantidade"] = "6",
            ["Itens[0].ValorUnitario"] = "99.90",
            ["Itens[0].NomeModelo"] = "Tênis XML",
            ["Itens[0].Marca"] = "Fábrica Teste",
            ["Itens[0].Categoria"] = "Calçado",
            ["Itens[0].Cor"] = "Preto",
            ["Itens[0].Numeracao"] = "38",
            ["__RequestVerificationToken"] = await TokenAsync(reviewPage)
        });
        using var confirmResponse = await client.PostAsync("/EntradasFiscais/Confirmar", confirmForm);
        Assert.Equal(HttpStatusCode.Found, confirmResponse.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            var entrada = await context.EntradaFiscal.Include(e => e.Itens).SingleAsync(e => e.Id == entradaId);
            Assert.Equal(StatusEntradaFiscal.CONFIRMADA, entrada.Status);
            var produto = await context.Produto.Include(p => p.Skus).SingleAsync(p => p.Nome == "Tênis XML");
            var sku = produto.Skus.Single(s => s.Numeracao == "38");
            Assert.Equal(6, sku.SaldoAtual);
            Assert.NotNull(sku.UltimaAtualizacaoSaldoEm);
            Assert.Equal(DateTimeKind.Utc, sku.UltimaAtualizacaoSaldoEm.Value.Kind);
            var movimento = await context.Movimentacao.SingleAsync(m => m.SkuId == sku.Id);
            Assert.Equal(TipoMovimentacao.ENTRADA, movimento.Tipo);
            Assert.Equal(6, movimento.Quantidade);
            Assert.Contains("NF-e 1234", movimento.Motivo);
            Assert.Equal(sku.Id, await context.VinculoProdutoFornecedor
                .Where(v => v.CnpjFornecedor == "12345678000123" && v.CodigoFornecedor == "REF-PTO-38")
                .Select(v => v.SkuId).SingleAsync());
        }

        using var duplicatePage = await client.GetAsync("/EntradasFiscais/Importar");
        using var duplicateForm = FormularioXml(await TokenAsync(duplicatePage));
        using var duplicateResponse = await client.PostAsync("/EntradasFiscais/Importar", duplicateForm);
        Assert.Equal(HttpStatusCode.OK, duplicateResponse.StatusCode);
        Assert.Contains("já foi confirmada", WebUtility.HtmlDecode(await duplicateResponse.Content.ReadAsStringAsync()));
    }

    private static MultipartFormDataContent FormularioXml(string token)
    {
        var xml = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <nfeProc xmlns="http://www.portalfiscal.inf.br/nfe">
              <NFe><infNFe Id="NFe{Chave}">
                <ide><nNF>1234</nNF></ide>
                <emit><CNPJ>12345678000123</CNPJ><xNome>Fábrica Teste</xNome></emit>
                <det nItem="1"><prod><cProd>REF-PTO-38</cProd><cEAN>SEM GTIN</cEAN><xProd>TÊNIS XML PRETO 38</xProd><qCom>6.0000</qCom><vUnCom>99.90</vUnCom></prod></det>
              </infNFe></NFe>
              <protNFe><infProt><chNFe>{Chave}</chNFe></infProt></protNFe>
            </nfeProc>
            """;
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(token), "__RequestVerificationToken");
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes(xml));
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
        form.Add(file, "ArquivoXml", "nota.xml");
        return form;
    }

    private static HttpClient CreateClient(SquadEstoqueWebApplicationFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });

    private static async Task LoginAsync(HttpClient client, string email = "lojista@squad.com")
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
        var match = Regex.Match(await page.Content.ReadAsStringAsync(),
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"[^>]*>");
        Assert.True(match.Success);
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }
}
