using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class MovimentacaoSomenteLeituraHttpTests
{
    [Fact]
    public async Task Movimentacao_registrada_nao_pode_ser_alterada_ou_excluida_pelas_rotas_mvc()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        using var loginPage = await client.GetAsync("/Account/Login");
        using var loginForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "lojista@squad.com",
            ["Senha"] = "123",
            ["__RequestVerificationToken"] = await TokenAsync(loginPage)
        });
        using var login = await client.PostAsync("/Account/Login", loginForm);
        Assert.Equal(HttpStatusCode.Found, login.StatusCode);

        Guid skuId;
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            skuId = await context.Sku.Where(s => s.Numeracao == "39").Select(s => s.Id).SingleAsync();
        }

        using var entradaPage = await client.GetAsync($"/Movimentacoes/Entrada?skuId={skuId}");
        using var entradaForm = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["SkuId"] = skuId.ToString(),
            ["Quantidade"] = "1",
            ["Motivo"] = "Registro original",
            ["__RequestVerificationToken"] = await TokenAsync(entradaPage)
        });
        using var entrada = await client.PostAsync("/Movimentacoes/Entrada", entradaForm);
        Assert.Equal(HttpStatusCode.Found, entrada.StatusCode);

        var original = Assert.Single(await SnapshotAsync(factory));
        Assert.Equal("Registro original", original.Motivo);

        var actions = factory.Services.GetRequiredService<IActionDescriptorCollectionProvider>()
            .ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
            .Where(action => action.ControllerName == "Movimentacoes")
            .Select(action => action.ActionName)
            .Distinct()
            .ToArray();
        Assert.Equal(new[] { "Ajuste", "Entrada", "EntradaLote", "Index", "Saida" },
            actions.OrderBy(action => action));

        using var historico = await client.GetAsync("/Movimentacoes");
        Assert.Equal(HttpStatusCode.OK, historico.StatusCode);
        var html = WebUtility.HtmlDecode(await historico.Content.ReadAsStringAsync());
        Assert.Contains("Registro original", html);
        Assert.DoesNotMatch(@"(?i)(?:href|action)=""/Movimentacoes/(?:Edit|Delete|Editar|Excluir)", html);

        var camposAlterados = new Dictionary<string, string>
        {
            ["Id"] = original.Id.ToString(),
            ["SkuId"] = Guid.NewGuid().ToString(),
            ["Quantidade"] = "99",
            ["Motivo"] = "Tentativa de alteração",
            ["__RequestVerificationToken"] = await TokenAsync(entradaPage)
        };

        foreach (var action in new[] { "Edit", "Delete", "Editar", "Excluir" })
        foreach (var url in new[]
        {
            $"/Movimentacoes/{action}/{original.Id}",
            $"/Movimentacoes/{action}?id={original.Id}"
        })
        foreach (var method in new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Patch, HttpMethod.Delete })
        {
            using var request = new HttpRequestMessage(method, url);
            if (method != HttpMethod.Get)
                request.Content = new FormUrlEncodedContent(camposAlterados);

            using var response = await client.SendAsync(request);
            Assert.True(response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.MethodNotAllowed,
                $"{method} {url}: esperado 404 ou 405, recebido {(int)response.StatusCode}.");
            Assert.Equal(new[] { original }, await SnapshotAsync(factory));
        }
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
