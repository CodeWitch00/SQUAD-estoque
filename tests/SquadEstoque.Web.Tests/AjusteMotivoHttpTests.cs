using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class AjusteMotivoHttpTests
{
    private const string Route = "/Movimentacoes/Ajuste";
    private static readonly Guid ProdutoId = Guid.Parse("02000000-0000-0000-0000-000000000001");
    private static readonly Guid SkuId = Guid.Parse("02000000-0000-0000-0000-000000000002");

    [Theory]
    [InlineData(null, "O motivo do ajuste é obrigatório.")]
    [InlineData("", "O motivo do ajuste é obrigatório.")]
    [InlineData("     ", "O motivo do ajuste é obrigatório.")]
    [InlineData("abcd", "O motivo deve conter pelo menos 5 caracteres.")]
    [InlineData("  abcd  ", "O motivo deve conter pelo menos 5 caracteres.")]
    [InlineData(" \t\r\n\u00a0 ", "O motivo do ajuste é obrigatório.")]
    [InlineData(" a   b ", "O motivo deve conter pelo menos 5 caracteres.")]
    [MemberData(nameof(MotivosAcimaDoLimite))]
    public async Task Motivo_invalido_rejeita_ajuste_sem_alterar_saldos_ou_historico(string? motivo, string erroEsperado)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        await SeedAsync(factory);
        using var client = CreateClient(factory);
        await LoginAsync(client);
        var before = await ReadStateAsync(factory);
        var token = await GetTokenAsync(client, $"{Route}?skuId={SkuId}");
        using var form = AdjustmentForm(token, motivo, 15);

        using var response = await client.PostAsync(Route, form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Contains(erroEsperado, html);
        var after = await ReadStateAsync(factory);
        Assert.Equal(before.Saldos, after.Saldos);
        Assert.Equal(before.Movimentos, after.Movimentos);
    }

    public static IEnumerable<object[]> MotivosAcimaDoLimite =>
        new[] { new object[] { new string('a', 501), "O motivo não pode exceder 500 caracteres." } };

    public static IEnumerable<object[]> MotivosNoLimite => new[]
    {
        new object[] { new string('a', 500), new string('a', 500), 15, 5 },
        new object[] { "  " + new string('a', 500) + "  ", new string('a', 500), 15, 5 }
    };

    [Theory]
    [InlineData("  abcde  ", "abcde", 15, 5)]
    [InlineData("  Contagem física  ", "Contagem física", 15, 5)]
    [InlineData("  Correção de inventário  ", "Correção de inventário", 7, 3)]
    [InlineData(" \tContagem   física\r\n de\u00a0 estoque  ", "Contagem física de estoque", 15, 5)]
    [MemberData(nameof(MotivosNoLimite))]
    public async Task Motivo_valido_persiste_normalizado_com_diferenca_usuario_e_data(
        string motivo, string esperado, int novoSaldo, int diferenca)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        var (lojistaId, vendedorId) = await SeedAsync(factory);
        using var client = CreateClient(factory);
        await LoginAsync(client);
        var before = await ReadStateAsync(factory);
        var token = await GetTokenAsync(client, $"{Route}?skuId={SkuId}");
        using var form = AdjustmentForm(token, motivo, novoSaldo, vendedorId);
        var inicio = DateTime.UtcNow;

        using var response = await client.PostAsync(Route, form);

        var fim = DateTime.UtcNow;
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal($"/Produtos/Details/{ProdutoId}", response.Headers.Location?.OriginalString);
        var after = await ReadStateAsync(factory);
        Assert.Equal(before.Saldos.Select(s => s.Id == SkuId ? s with { SaldoAtual = novoSaldo } : s), after.Saldos);
        Assert.Equal(before.Movimentos, after.Movimentos.Where(m => before.Movimentos.Any(b => b.Id == m.Id)));
        Assert.Equal(before.Movimentos.Length + 1, after.Movimentos.Length);
        var movimento = Assert.Single(after.Movimentos, m => before.Movimentos.All(b => b.Id != m.Id));
        Assert.NotEqual(Guid.Empty, movimento.Id);
        Assert.Equal(SkuId, movimento.SkuId);
        Assert.Equal(TipoMovimentacao.AJUSTE, movimento.Tipo);
        Assert.Equal(diferenca, movimento.Quantidade);
        Assert.Equal(esperado, movimento.Motivo);
        Assert.Equal(lojistaId, movimento.UsuarioId);
        Assert.InRange(movimento.CriadoEm, inicio, fim);
    }

    [Fact]
    public async Task Saldo_inalterado_rejeita_ajuste_mesmo_com_motivo_valido()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        await SeedAsync(factory);
        using var client = CreateClient(factory);
        await LoginAsync(client);
        var before = await ReadStateAsync(factory);
        var token = await GetTokenAsync(client, $"{Route}?skuId={SkuId}");
        using var form = AdjustmentForm(token, "Contagem física", 10);

        using var response = await client.PostAsync(Route, form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Não há alteração de saldo a ser registrada.",
            WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()));
        var after = await ReadStateAsync(factory);
        Assert.Equal(before.Saldos, after.Saldos);
        Assert.Equal(before.Movimentos, after.Movimentos);
    }

    private static FormUrlEncodedContent AdjustmentForm(string token, string? motivo, int saldo, Guid? usuarioEnviado = null)
    {
        var fields = new Dictionary<string, string>
        {
            ["SkuId"] = SkuId.ToString(),
            ["NovoSaldoApurado"] = saldo.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["__RequestVerificationToken"] = token
        };
        if (motivo is not null) fields["Motivo"] = motivo;
        // Identidade e data devem vir do servidor, mesmo quando o cliente envia outros valores.
        if (usuarioEnviado.HasValue)
        {
            fields["UsuarioId"] = usuarioEnviado.Value.ToString();
            fields["CriadoEm"] = "2000-01-01T00:00:00Z";
        }
        return new FormUrlEncodedContent(fields);
    }

    private static async Task<(Guid Lojista, Guid Vendedor)> SeedAsync(SquadEstoqueWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var lojista = await db.Usuario.SingleAsync(u => u.Email == "lojista@squad.com");
        var vendedor = await db.Usuario.SingleAsync(u => u.Email == "vendedor@squad.com");
        var produto = new Produto
        {
            Id = ProdutoId, Nome = "Tênis QA-020", Marca = "Squad", Categoria = "Calçado", Cor = "Preto"
        };
        produto.Skus.Add(new Sku { Id = SkuId, Numeracao = "40", SaldoAtual = 10 });
        db.Produto.Add(produto);
        db.Movimentacao.Add(new Movimentacao
        {
            Id = Guid.Parse("02000000-0000-0000-0000-000000000003"),
            SkuId = SkuId, UsuarioId = lojista.Id, Tipo = TipoMovimentacao.ENTRADA,
            Quantidade = 10, Motivo = "Entrada anterior preservada",
            CriadoEm = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc)
        });
        await db.SaveChangesAsync();
        return (lojista.Id, vendedor.Id);
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

    private static HttpClient CreateClient(SquadEstoqueWebApplicationFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });

    private static async Task LoginAsync(HttpClient client)
    {
        var token = await GetTokenAsync(client, "/Account/Login");
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "lojista@squad.com", ["Senha"] = "123", ["__RequestVerificationToken"] = token
        });
        using var response = await client.PostAsync("/Account/Login", form);
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
    }

    private static async Task<string> GetTokenAsync(HttpClient client, string route)
    {
        using var response = await client.GetAsync(route);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(html, "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"[^>]*>");
        Assert.True(match.Success, "Token antiforgery não encontrado.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }
}
