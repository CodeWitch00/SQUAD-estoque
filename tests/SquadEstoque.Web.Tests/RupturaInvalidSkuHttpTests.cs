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

public sealed class RupturaInvalidSkuHttpTests
{
    [Theory]
    [InlineData("absent", "Selecione uma numeração para registrar a ruptura.")]
    [InlineData("empty", "Selecione uma numeração para registrar a ruptura.")]
    [InlineData("malformed", "Selecione uma numeração para registrar a ruptura.")]
    [InlineData("zero", "Selecione uma numeração para registrar a ruptura.")]
    [InlineData("unknown", "A numeração selecionada não está disponível para registro.")]
    [InlineData("other-product", "A numeração selecionada não está disponível para registro.")]
    [InlineData("product-absent", "A numeração selecionada não está disponível para registro.")]
    [InlineData("product-malformed", "A numeração selecionada não está disponível para registro.")]
    [InlineData("product-zero", "A numeração selecionada não está disponível para registro.")]
    [InlineData("product-unknown", "A numeração selecionada não está disponível para registro.")]
    public async Task Invalid_sku_is_rejected_without_disclosure_or_persisted_changes(
        string scenario, string expectedMessage)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false, HandleCookies = true
        });
        Guid consultedProductId;
        Guid consultedSkuId;
        var otherProductId = Guid.NewGuid();
        var otherSkuId = Guid.NewGuid();
        var missingSkuId = Guid.NewGuid();
        Dictionary<Guid, int> balancesBefore;
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            consultedProductId = await context.Produto.Select(p => p.Id).SingleAsync();
            consultedSkuId = await context.Sku.Where(s => s.ProdutoId == consultedProductId && s.Ativo)
                .Select(s => s.Id).FirstAsync();
            var otherProduct = new Produto
            {
                Id = otherProductId, Nome = "Produto reservado QA", Marca = "Marca reservada",
                Categoria = "Categoria reservada", Cor = "Cor reservada", Ativo = true
            };
            otherProduct.Skus.Add(new Sku
            {
                Id = otherSkuId, ProdutoId = otherProductId, Numeracao = "42",
                SaldoAtual = 7, Ativo = true
            });
            context.Produto.Add(otherProduct);
            await context.SaveChangesAsync();
            Assert.True(await context.Sku.AnyAsync(s => s.Id == otherSkuId && s.Ativo && s.Produto!.Ativo));
            Assert.False(await context.Sku.AnyAsync(s => s.Id == missingSkuId));
            balancesBefore = await context.Sku.AsNoTracking().ToDictionaryAsync(s => s.Id, s => s.SaldoAtual);
            Assert.Contains(0, balancesBefore.Values);
            Assert.Contains(1, balancesBefore.Values);
            Assert.Contains(7, balancesBefore.Values);
            Assert.Empty(await context.Ruptura.AsNoTracking().ToListAsync());
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

        // Load the actual selected product before attempting a SKU substitution.
        using var consultation = await client.GetAsync(
            $"/Estoque/Consulta?termo=Runner&produtoId={consultedProductId}");
        var consultationHtml = await consultation.Content.ReadAsStringAsync();
        Assert.Contains("Tênis Runner", WebUtility.HtmlDecode(consultationHtml));
        Assert.Contains($"data-produto-id=\"{consultedProductId}\"", consultationHtml);
        Assert.DoesNotContain(otherSkuId.ToString(), consultationHtml);
        var formValues = new Dictionary<string, string>
        {
            ["produtoId"] = consultedProductId.ToString(),
            ["__RequestVerificationToken"] = await TokenAsync(consultation)
        };
        if (scenario != "absent")
            formValues["skuId"] = scenario switch
            {
                "empty" => "",
                "malformed" => "sku-invalido",
                "zero" => Guid.Empty.ToString(),
                "unknown" => missingSkuId.ToString(),
                "other-product" => otherSkuId.ToString(),
                "product-absent" or "product-malformed" or "product-zero" or "product-unknown" => consultedSkuId.ToString(),
                _ => throw new ArgumentOutOfRangeException(nameof(scenario))
            };
        if (scenario == "product-absent") formValues.Remove("produtoId");
        if (scenario == "product-malformed") formValues["produtoId"] = "produto-invalido";
        if (scenario == "product-zero") formValues["produtoId"] = Guid.Empty.ToString();
        if (scenario == "product-unknown") formValues["produtoId"] = Guid.NewGuid().ToString();
        using var form = new FormUrlEncodedContent(formValues);
        using var response = await client.PostAsync("/Estoque/RegistrarNaoTinha", form);

        // Collect all observations before asserting, including a regression that returns 200.
        var body = await response.Content.ReadAsStringAsync();
        using var verificationScope = factory.Services.CreateScope();
        var persisted = verificationScope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var balancesAfter = await persisted.Sku.AsNoTracking().ToDictionaryAsync(s => s.Id, s => s.SaldoAtual);
        var ruptures = await persisted.Ruptura.AsNoTracking().ToListAsync();
        var movements = await persisted.Movimentacao.AsNoTracking().ToListAsync();
        Assert.Equal(balancesBefore.OrderBy(s => s.Key), balancesAfter.OrderBy(s => s.Key));
        Assert.Empty(movements);
        Assert.Empty(ruptures);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Null(response.Headers.Location);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(JsonValueKind.Object, payload.ValueKind);
        var property = Assert.Single(payload.EnumerateObject());
        Assert.Equal("mensagem", property.Name);
        Assert.Equal(expectedMessage, property.Value.GetString());
        // The complete response schema permits only the public validation message.
        Assert.DoesNotContain(otherSkuId.ToString(), body);
        Assert.DoesNotContain(otherProductId.ToString(), body);
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
