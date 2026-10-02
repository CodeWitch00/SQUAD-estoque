using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class DesistiuHttpTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Desistiu_ends_context_without_persistence_and_allows_another_query(bool enviarSku)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        var before = await SnapshotAsync(factory);
        await LoginAsync(client, "vendedor@squad.com");
        using var grade = await client.GetAsync("/Estoque/Consulta?termo=Runner&produtoId=aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var html = await grade.Content.ReadAsStringAsync();
        Assert.Contains("data-desistiu-url=\"/Estoque/RegistrarDesistiu\"", html);
        var values = new Dictionary<string, string> { ["__RequestVerificationToken"] = await TokenAsync(grade) };
        // A selected SKU is optional context and must never trigger a stock operation.
        if (enviarSku) values["skuId"] = before.Saldos.First().Key.ToString();
        for (var tentativa = 0; tentativa < 2; tentativa++)
        {
            using var form = new FormUrlEncodedContent(values);
            using var response = await client.PostAsync("/Estoque/RegistrarDesistiu", form);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("desistiu", payload.GetProperty("resultado").GetString());
            Assert.True(payload.GetProperty("atendimentoEncerrado").GetBoolean());
            Assert.Equal("Atendimento encerrado.", payload.GetProperty("mensagem").GetString());
            Assert.Equal("/Estoque/Consulta", payload.GetProperty("novaConsultaUrl").GetString());
        }
        using var novaConsulta = await client.GetAsync("/Estoque/Consulta");
        var novaHtml = await novaConsulta.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, novaConsulta.StatusCode);
        Assert.DoesNotContain("id=\"grade\"", novaHtml);
        Assert.Contains("value=\"\"", novaHtml);
        using var outraGrade = await client.GetAsync("/Estoque/Consulta?termo=Runner&produtoId=aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        Assert.Contains("id=\"grade\"", await outraGrade.Content.ReadAsStringAsync());
        await AssertUnchangedAsync(factory, before);
    }

    [Theory]
    [InlineData(null, "/Account/Login", "/Account/Login?ReturnUrl=%2FEstoque%2FRegistrarDesistiu")]
    [InlineData("lojista@squad.com", "/Produtos", "/Account/AccessDenied?ReturnUrl=%2FEstoque%2FRegistrarDesistiu")]
    public async Task Desistiu_requires_authenticated_seller(string? email, string tokenPage, string redirect)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        var before = await SnapshotAsync(factory);
        if (email is not null) await LoginAsync(client, email);
        using var page = await client.GetAsync(tokenPage);
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = await TokenAsync(page)
        });
        using var response = await client.PostAsync("/Estoque/RegistrarDesistiu", form);
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal(redirect, response.Headers.Location?.PathAndQuery);
        await AssertUnchangedAsync(factory, before);
    }

    [Fact]
    public async Task Desistiu_rejects_missing_antiforgery_and_get()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        var before = await SnapshotAsync(factory);
        await LoginAsync(client, "vendedor@squad.com");
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>());
        using var post = await client.PostAsync("/Estoque/RegistrarDesistiu", form);
        Assert.Equal(HttpStatusCode.BadRequest, post.StatusCode);
        using var get = await client.GetAsync("/Estoque/RegistrarDesistiu");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        await AssertUnchangedAsync(factory, before);
    }

    private static HttpClient CreateClient(SquadEstoqueWebApplicationFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });

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
        var match = Regex.Match(await page.Content.ReadAsStringAsync(),
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"[^>]*>");
        Assert.True(match.Success);
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static async Task<(Dictionary<Guid, int> Saldos, int Movimentacoes, int Rupturas)> SnapshotAsync(
        SquadEstoqueWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        return (await context.Sku.AsNoTracking().ToDictionaryAsync(s => s.Id, s => s.SaldoAtual),
            await context.Movimentacao.CountAsync(), await context.Ruptura.CountAsync());
    }

    private static async Task AssertUnchangedAsync(SquadEstoqueWebApplicationFactory factory,
        (Dictionary<Guid, int> Saldos, int Movimentacoes, int Rupturas) before)
    {
        var after = await SnapshotAsync(factory);
        Assert.Equal(before.Saldos.OrderBy(s => s.Key), after.Saldos.OrderBy(s => s.Key));
        Assert.Equal(before.Movimentacoes, after.Movimentacoes);
        Assert.Equal(before.Rupturas, after.Rupturas);
    }
}
