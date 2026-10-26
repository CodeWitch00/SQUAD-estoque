using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class MovimentacaoImutabilidadeHttpTests
{
    public static IEnumerable<object[]> Cenarios()
    {
        foreach (var tipo in Enum.GetValues<TipoMovimentacao>())
        foreach (var perfil in new[] { "LOJISTA", "VENDEDOR", "ANONIMO" })
            yield return new object[] { tipo, perfil };
    }

    [Theory]
    [MemberData(nameof(Cenarios))]
    public async Task Persisted_movement_cannot_be_edited_or_deleted_through_application(
        TipoMovimentacao tipo, string perfil)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var lojista = CreateClient(factory);
        await LoginAsync(lojista, "lojista@squad.com");

        // Only read the factory seed. Create the movement through the application.
        Guid skuId;
        Guid outroSkuId;
        Guid outroUsuarioId;
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            skuId = await context.Sku.Where(s => s.Numeracao == "39").Select(s => s.Id).SingleAsync();
            outroSkuId = await context.Sku.Where(s => s.Numeracao == "38").Select(s => s.Id).SingleAsync();
            outroUsuarioId = await context.Usuario.Where(u => u.Email == "vendedor@squad.com")
                .Select(u => u.Id).SingleAsync();
        }

        var acao = tipo switch
        {
            TipoMovimentacao.ENTRADA => "Entrada",
            TipoMovimentacao.SAIDA => "Saida",
            _ => "Ajuste"
        };
        var motivo = $"Imutabilidade QA {tipo}";
        using var formulario = await lojista.GetAsync($"/Movimentacoes/{acao}?skuId={skuId}");
        using var dados = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["SkuId"] = skuId.ToString(), ["Quantidade"] = "1", ["NovoSaldoApurado"] = "5",
            ["Motivo"] = motivo, ["__RequestVerificationToken"] = await TokenAsync(formulario)
        });
        using var criacao = await lojista.PostAsync($"/Movimentacoes/{acao}", dados);
        Assert.Equal(HttpStatusCode.Found, criacao.StatusCode);
        Assert.StartsWith("/Produtos/Details/", criacao.Headers.Location?.OriginalString);

        var antes = await SnapshotAsync(factory);
        var original = Assert.Single(antes);
        Assert.NotEqual(Guid.Empty, original.Id);
        Assert.Equal(skuId, original.SkuId);
        Assert.Equal(tipo, original.Tipo);
        Assert.Equal(tipo == TipoMovimentacao.AJUSTE ? 3 : 1, original.Quantidade);
        Assert.Equal(motivo, original.Motivo);
        await AssertHistoryAsync(lojista, motivo);

        using var tentativa = CreateClient(factory);
        if (perfil != "ANONIMO")
            await LoginAsync(tentativa, perfil == "LOJISTA" ? "lojista@squad.com" : "vendedor@squad.com");

        using var paginaToken = await tentativa.GetAsync(perfil == "ANONIMO"
            ? "/Account/Login" : perfil == "LOJISTA" ? "/Movimentacoes/Entrada" : "/Estoque/Consulta");
        var token = await TokenAsync(paginaToken);
        var adulteracao = new Dictionary<string, string>
        {
            ["Id"] = original.Id.ToString(), ["SkuId"] = outroSkuId.ToString(),
            ["Tipo"] = ((int)(tipo == TipoMovimentacao.ENTRADA
                ? TipoMovimentacao.SAIDA : TipoMovimentacao.ENTRADA)).ToString(),
            ["Quantidade"] = "99", ["UsuarioId"] = outroUsuarioId.ToString(),
            ["CriadoEm"] = "2000-01-01T00:00:00Z", ["Motivo"] = "Tentativa de reescrita",
            ["AtendimentoId"] = Guid.NewGuid().ToString(), ["__RequestVerificationToken"] = token
        };

        var operacoes = new[] { "Edit", "Delete", "DeleteConfirmed", "Editar", "Excluir" };
        var actions = factory.Services.GetRequiredService<IActionDescriptorCollectionProvider>();
        Assert.DoesNotContain(actions.ActionDescriptors.Items.OfType<ControllerActionDescriptor>(),
            a => a.ControllerName == "Movimentacoes" && operacoes.Contains(a.ActionName, StringComparer.OrdinalIgnoreCase));

        // Conventional MVC names, localized names, and both ID binding conventions.
        foreach (var operacao in operacoes)
        foreach (var url in new[]
        {
            $"/Movimentacoes/{operacao}/{original.Id}",
            $"/Movimentacoes/{operacao}?id={original.Id}"
        })
        foreach (var metodo in new[] { HttpMethod.Get, HttpMethod.Post })
        {
            using var request = new HttpRequestMessage(metodo, url);
            if (metodo == HttpMethod.Post)
                request.Content = new FormUrlEncodedContent(adulteracao);
            using var response = await tentativa.SendAsync(request);
            Assert.True(response.StatusCode == HttpStatusCode.NotFound ||
                (metodo == HttpMethod.Post && response.StatusCode == HttpStatusCode.MethodNotAllowed),
                $"{perfil} {tipo} {metodo} {url}: esperado 404 (ou 405 para POST), recebido {(int)response.StatusCode}.");

            // Reload committed fields after every attempt, in a new context.
            Assert.Equal(antes, await SnapshotAsync(factory));
            await AssertHistoryAsync(lojista, motivo);
        }

        using var historico = await tentativa.GetAsync("/Movimentacoes");
        if (perfil == "LOJISTA")
        {
            Assert.Equal(HttpStatusCode.OK, historico.StatusCode);
            Assert.Contains(motivo, WebUtility.HtmlDecode(await historico.Content.ReadAsStringAsync()));
        }
        else
        {
            Assert.Equal(HttpStatusCode.Found, historico.StatusCode);
            Assert.StartsWith(perfil == "ANONIMO" ? "/Account/Login" : "/Account/AccessDenied",
                historico.Headers.Location?.AbsolutePath);
        }
        Assert.Equal(antes, await SnapshotAsync(factory));
    }

    private static HttpClient CreateClient(SquadEstoqueWebApplicationFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false, HandleCookies = true
        });

    private static async Task LoginAsync(HttpClient client, string email)
    {
        using var page = await client.GetAsync("/Account/Login");
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email, ["Senha"] = "123",
            ["__RequestVerificationToken"] = await TokenAsync(page)
        });
        using var response = await client.PostAsync("/Account/Login", form);
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal(email == "lojista@squad.com" ? "/Produtos" : "/Estoque/Consulta",
            response.Headers.Location?.OriginalString);
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

    private static async Task AssertHistoryAsync(HttpClient lojista, string motivo)
    {
        using var response = await lojista.GetAsync("/Movimentacoes");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Contains(motivo, html);
        Assert.DoesNotMatch(@"(?i)(?:href|action)=""/Movimentacoes/(?:Edit|Delete|Editar|Excluir)", html);
    }

    private static async Task<MovimentoSnapshot[]> SnapshotAsync(SquadEstoqueWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        return await context.Movimentacao.AsNoTracking().OrderBy(m => m.Id)
            .Select(m => new MovimentoSnapshot(m.Id, m.SkuId, m.Tipo, m.Quantidade,
                m.UsuarioId, m.CriadoEm, m.Motivo, m.AtendimentoId)).ToArrayAsync();
    }

    private sealed record MovimentoSnapshot(Guid Id, Guid SkuId, TipoMovimentacao Tipo,
        int Quantidade, Guid UsuarioId, DateTime CriadoEm, string? Motivo, Guid? AtendimentoId);
}
