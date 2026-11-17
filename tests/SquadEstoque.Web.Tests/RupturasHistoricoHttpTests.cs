using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class RupturasHistoricoHttpTests
{
    [Fact]
    public async Task Historico_http_exibe_contagem_igual_aos_registros_persistidos()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            var produto = await db.Produto.Include(item => item.Skus)
                .SingleAsync(item => item.Nome == "Tênis Runner");
            var usuario = await db.Usuario.SingleAsync(item => item.Email == "vendedor@squad.com");
            var skus = produto.Skus.ToDictionary(item => item.Numeracao);
            var data = new DateTime(2026, 10, 29, 12, 0, 0, DateTimeKind.Utc);

            db.Ruptura.AddRange(
                Enumerable.Range(0, 5).Select(i => new Ruptura
                {
                    Id = Guid.NewGuid(), SkuId = skus["38"].Id, UsuarioId = usuario.Id, CriadoEm = data.AddMinutes(i)
                })
                .Concat(Enumerable.Range(0, 3).Select(i => new Ruptura
                {
                    Id = Guid.NewGuid(), SkuId = skus["39"].Id, UsuarioId = usuario.Id, CriadoEm = data.AddMinutes(i)
                }))
                .Append(new Ruptura
                {
                    Id = Guid.NewGuid(), SkuId = skus["37"].Id, UsuarioId = usuario.Id, CriadoEm = data
                }));
            await db.SaveChangesAsync();
        }

        await LoginAsync(client, "lojista@squad.com");
        using var response = await client.GetAsync("/Rupturas");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var numeracoesExibidas = Regex.Matches(html,
                "data-label=\"Numeração\"[^>]*>\\s*(\\d+)")
            .Select(match => match.Groups[1].Value)
            .ToArray();
        var contagensExibidas = Regex.Matches(html, "<strong[^>]*>(\\d+)</strong>")
            .Select(match => int.Parse(match.Groups[1].Value))
            .ToArray();
        Assert.Equal(new[] { "38", "39", "37" }, numeracoesExibidas);
        Assert.Equal(new[] { 5, 3, 1 }, contagensExibidas);

        using var verifyScope = factory.Services.CreateScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var contagensPersistidas = await verify.Ruptura
            .GroupBy(item => item.Sku!.Numeracao)
            .Select(grupo => new { Numeracao = grupo.Key, Total = grupo.Count() })
            .ToDictionaryAsync(item => item.Numeracao, item => item.Total);
        Assert.Equal(9, await verify.Ruptura.CountAsync());
        Assert.Equal(5, contagensPersistidas["38"]);
        Assert.Equal(3, contagensPersistidas["39"]);
        Assert.Equal(1, contagensPersistidas["37"]);
    }

    [Fact]
    public async Task Historico_agrega_ordena_e_preserva_registros_apos_reposicao()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            var skus = await db.Sku.OrderBy(s => s.Numeracao).ToListAsync();
            var usuario = await db.Usuario.SingleAsync(u => u.Email == "vendedor@squad.com");
            var data = new DateTime(2026, 10, 20, 12, 30, 0, DateTimeKind.Utc);
            // 38 tem mais ocorrências; 39 desempata pela data. 37 tem saldo reposto.
            skus[0].SaldoAtual = 7;
            db.Ruptura.AddRange(
                new Ruptura { Id = Guid.NewGuid(), SkuId = skus[0].Id, UsuarioId = usuario.Id, CriadoEm = data.AddDays(-2) },
                new Ruptura { Id = Guid.NewGuid(), SkuId = skus[1].Id, UsuarioId = usuario.Id, CriadoEm = data.AddDays(-3) },
                new Ruptura { Id = Guid.NewGuid(), SkuId = skus[1].Id, UsuarioId = usuario.Id, CriadoEm = data.AddDays(-1) },
                new Ruptura { Id = Guid.NewGuid(), SkuId = skus[2].Id, UsuarioId = usuario.Id, CriadoEm = data });
            await db.SaveChangesAsync();
        }
        await LoginAsync(client, "lojista@squad.com");
        var response = await client.GetAsync("/Rupturas");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Histórico de rupturas", html);
        Assert.Contains("não representam o saldo atual", html);
        Assert.Contains("Tênis Runner", html);
        Assert.Contains("Squad · Preto", html);
        Assert.Contains("19/10/2026 12:30", html);
        Assert.Matches("<strong[^>]*>2</strong>", html);
        var numeracoes = Regex.Matches(html, "data-label=\"Numeração\"[^>]*>([^<]+)</td>")
            .Select(m => m.Groups[1].Value).ToArray();
        Assert.Equal(new[] { "38", "39", "37" }, numeracoes);
        Assert.Matches("aria-current=\"page\"[^>]*>Histórico de rupturas", html);
        Assert.DoesNotContain("/Rupturas/Edit", html);
        Assert.DoesNotContain("/Rupturas/Delete", html);
        using var verifyScope = factory.Services.CreateScope();
        var verify = verifyScope.ServiceProvider.GetRequiredService<EstoqueContext>();
        Assert.Equal(4, await verify.Ruptura.CountAsync());
        Assert.Equal(7, (await verify.Sku.SingleAsync(s => s.Numeracao == "37")).SaldoAtual);
    }

    [Fact]
    public async Task Historico_vazio_exibe_orientacao()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        await LoginAsync(client, "lojista@squad.com");
        var html = WebUtility.HtmlDecode(await client.GetStringAsync("/Rupturas"));
        Assert.Contains("Nenhuma ruptura registrada", html);
        Assert.Contains("Quando uma falta for registrada no atendimento", html);
        Assert.DoesNotContain("<table", html);
    }

    [Theory]
    [InlineData(null, "/Account/Login")]
    [InlineData("vendedor@squad.com", "/Account/AccessDenied")]
    public async Task Historico_restrito_ao_lojista(string? email, string destino)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        if (email != null) await LoginAsync(client, email);
        var response = await client.GetAsync("/Rupturas");
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith(destino, response.Headers.Location?.PathAndQuery);
        var page = await client.GetStringAsync(email == null ? "/Account/Login" : "/Estoque/Consulta");
        Assert.DoesNotContain("href=\"/Rupturas\"", page);
    }

    private static HttpClient CreateClient(SquadEstoqueWebApplicationFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static async Task LoginAsync(HttpClient client, string email)
    {
        var html = await client.GetStringAsync("/Account/Login");
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email, ["Senha"] = "123", ["__RequestVerificationToken"] = WebUtility.HtmlDecode(token)
        });
        var response = await client.PostAsync("/Account/Login", form);
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
    }
}
