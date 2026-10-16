using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class AjusteAuthorizationHttpTests
{
    private const string Route = "/Movimentacoes/Ajuste";
    private const string Motivo = "Conferencia fisica do estoque";

    [Fact]
    public async Task Lojista_pode_abrir_ajuste_sem_alterar_saldo_ou_historico()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        await LoginAsync(client, "lojista@squad.com");
        var (skuId, _) = await SeedMovementAsync(factory);
        var before = await ReadStateAsync(factory);

        using var response = await client.GetAsync($"{Route}?skuId={skuId}");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("name=\"NovoSaldoApurado\"", html);
        Assert.Contains("name=\"Motivo\"", html);
        Assert.Contains(skuId.ToString(), html);
        ExtractToken(html);
        await AssertUnchangedAsync(factory, before);
    }

    [Fact]
    public async Task Lojista_pode_enviar_ajuste_e_registrar_movimentacao_com_sua_identidade()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        await LoginAsync(client, "lojista@squad.com");
        var (skuId, lojistaId) = await SeedMovementAsync(factory);
        var before = await ReadStateAsync(factory);
        var token = await GetTokenAsync(client, $"{Route}?skuId={skuId}");
        using var form = AdjustmentForm(skuId, token);

        using var response = await client.PostAsync(Route, form);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/Produtos/Details/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
            response.Headers.Location?.OriginalString);
        var after = await ReadStateAsync(factory);
        Assert.Equal(before.Saldos.Select(s => s.Id == skuId ? s with { SaldoAtual = 5 } : s), after.Saldos);
        Assert.Equal(before.Movimentos, after.Movimentos.Where(m => before.Movimentos.Any(b => b.Id == m.Id)));
        var movement = Assert.Single(after.Movimentos, m => before.Movimentos.All(b => b.Id != m.Id));
        Assert.Equal(skuId, movement.SkuId);
        Assert.Equal(lojistaId, movement.UsuarioId);
        Assert.Equal(TipoMovimentacao.AJUSTE, movement.Tipo);
        Assert.Equal(3, movement.Quantidade);
        Assert.Equal(Motivo, movement.Motivo);
    }

    [Theory]
    [InlineData("vendedor@squad.com", "GET", "/Account/AccessDenied")]
    [InlineData("vendedor@squad.com", "POST", "/Account/AccessDenied")]
    [InlineData(null, "GET", "/Account/Login")]
    [InlineData(null, "POST", "/Account/Login")]
    public async Task Acesso_negado_preserva_todos_os_saldos_e_movimentacoes(
        string? email, string method, string expectedPage)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        if (email is not null)
            await LoginAsync(client, email);
        var (skuId, _) = await SeedMovementAsync(factory);

        // Token da propria sessao: uma falha de antiforgery nao deve mascarar a autorizacao.
        var token = await GetTokenAsync(client, email is null ? "/Account/Login" : "/Movimentacoes/Saida");
        var before = await ReadStateAsync(factory);
        var requestPath = method == "GET" ? $"{Route}?skuId={skuId}" : Route;
        using var request = new HttpRequestMessage(new HttpMethod(method), requestPath);
        if (method == "POST")
            request.Content = AdjustmentForm(skuId, token);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal($"{expectedPage}?ReturnUrl={Uri.EscapeDataString(requestPath)}",
            response.Headers.Location?.PathAndQuery);
        await AssertUnchangedAsync(factory, before);
    }

    [Fact]
    public async Task Lojista_sem_token_antiforgery_nao_altera_saldo_ou_historico()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        await LoginAsync(client, "lojista@squad.com");
        var (skuId, _) = await SeedMovementAsync(factory);
        var before = await ReadStateAsync(factory);
        using var form = AdjustmentForm(skuId, null);

        using var response = await client.PostAsync(Route, form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertUnchangedAsync(factory, before);
    }

    private static FormUrlEncodedContent AdjustmentForm(Guid skuId, string? token)
    {
        var fields = new Dictionary<string, string>
        {
            ["SkuId"] = skuId.ToString(),
            ["NovoSaldoApurado"] = "5",
            ["Motivo"] = Motivo
        };
        if (token is not null)
            fields["__RequestVerificationToken"] = token;
        return new FormUrlEncodedContent(fields);
    }

    private static async Task<(Guid SkuId, Guid LojistaId)> SeedMovementAsync(
        SquadEstoqueWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var sku = await db.Sku.SingleAsync(s => s.Numeracao == "39");
        var lojistaId = await db.Usuario.Where(u => u.Email == "lojista@squad.com").Select(u => u.Id).SingleAsync();
        db.Movimentacao.Add(new Movimentacao
        {
            Id = Guid.NewGuid(), SkuId = sku.Id, UsuarioId = lojistaId,
            Tipo = TipoMovimentacao.ENTRADA, Quantidade = 2,
            Motivo = "Entrada anterior preservada", CriadoEm = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc)
        });
        await db.SaveChangesAsync();
        return (sku.Id, lojistaId);
    }

    private sealed record Saldo(Guid Id, int SaldoAtual);
    private sealed record Movimento(Guid Id, Guid SkuId, TipoMovimentacao Tipo, int Quantidade,
        Guid UsuarioId, DateTime CriadoEm, string? Motivo, Guid? AtendimentoId);
    private sealed record State(Saldo[] Saldos, Movimento[] Movimentos);

    private static async Task<State> ReadStateAsync(SquadEstoqueWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        return new State(
            await db.Sku.AsNoTracking().OrderBy(s => s.Id).Select(s => new Saldo(s.Id, s.SaldoAtual)).ToArrayAsync(),
            await db.Movimentacao.AsNoTracking().OrderBy(m => m.Id)
                .Select(m => new Movimento(m.Id, m.SkuId, m.Tipo, m.Quantidade, m.UsuarioId,
                    m.CriadoEm, m.Motivo, m.AtendimentoId)).ToArrayAsync());
    }

    private static async Task AssertUnchangedAsync(SquadEstoqueWebApplicationFactory factory, State before)
    {
        var after = await ReadStateAsync(factory);
        Assert.Equal(before.Saldos, after.Saldos);
        Assert.Equal(before.Movimentos, after.Movimentos);
    }

    private static HttpClient CreateClient(SquadEstoqueWebApplicationFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });

    private static async Task LoginAsync(HttpClient client, string email)
    {
        var token = await GetTokenAsync(client, "/Account/Login");
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email, ["Senha"] = "123", ["__RequestVerificationToken"] = token
        });
        using var response = await client.PostAsync("/Account/Login", form);
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        using var authenticatedPage = await client.GetAsync("/Movimentacoes/Saida");
        Assert.Equal(HttpStatusCode.OK, authenticatedPage.StatusCode);
    }

    private static async Task<string> GetTokenAsync(HttpClient client, string route)
    {
        using var response = await client.GetAsync(route);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return ExtractToken(await response.Content.ReadAsStringAsync());
    }

    private static string ExtractToken(string html)
    {
        var match = Regex.Match(html, "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"[^>]*>");
        Assert.True(match.Success, "Token antiforgery nao encontrado.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }
}
