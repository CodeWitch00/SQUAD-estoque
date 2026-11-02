using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class RelatoriosErroHttpTests
{
    [Theory]
    [InlineData("/SaldosZerados", "Produto")]
    [InlineData("/Rupturas", "Ruptura")]
    public async Task Falha_de_consulta_exibe_erro_sem_confundir_com_relatorio_vazio(string rota, string tabela)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var login = await client.GetStringAsync("/Account/Login");
        var token = Regex.Match(login, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = "lojista@squad.com", ["Senha"] = "123",
            ["__RequestVerificationToken"] = WebUtility.HtmlDecode(token)
        });
        using var autenticacao = await client.PostAsync("/Account/Login", form);
        Assert.Equal(HttpStatusCode.Found, autenticacao.StatusCode);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            // Banco em memória exclusivo deste teste: simula indisponibilidade real da consulta.
            await db.Database.ExecuteSqlRawAsync(tabela == "Produto"
                ? "ALTER TABLE Produto RENAME TO Indisponivel"
                : "ALTER TABLE Ruptura RENAME TO Indisponivel");
        }
        using var response = await client.GetAsync(rota);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Contains("role=\"alert\"", html);
        Assert.Contains("Não foi possível carregar o relatório", html);
        Assert.Contains("A falha na consulta não significa ausência de registros", html);
        Assert.Contains("Tentar novamente", html);
        Assert.Contains($"href=\"{rota}\"", html);
        Assert.DoesNotContain("Nenhum produto possui", html);
        Assert.DoesNotContain("Nenhuma ruptura registrada", html);
        Assert.DoesNotContain("SQLite Error", html);
    }
}
