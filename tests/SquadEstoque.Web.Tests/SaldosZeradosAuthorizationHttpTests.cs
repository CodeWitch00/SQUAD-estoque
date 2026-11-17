using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class SaldosZeradosAuthorizationHttpTests
{
    [Fact]
    public async Task Lojista_consulta_saldos_zerados_agrupados_sem_expor_skus_com_saldo()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        var nome = await AddProductAsync(factory);
        await LoginAsync(client, "lojista@squad.com");

        using var response = await client.GetAsync("/SaldosZerados");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Saldos zerados", html);
        Assert.Contains(nome, html);
        Assert.Contains("Numerações sem saldo", html);
        Assert.Contains(">47</span>", html);
        Assert.Contains("Atualizado em", html);
        Assert.Contains(new DateTime(2026, 10, 29, 14, 30, 0, DateTimeKind.Utc)
            .ToLocalTime().ToString("dd/MM/yyyy HH:mm"), html);
        Assert.Contains("Atualização não registrada", html);
        Assert.DoesNotContain(">48</span>", html);
        Assert.DoesNotContain(">49</span>", html);
        Assert.Contains("href=\"/SaldosZerados\"", html);
    }

    [Fact]
    public async Task Produto_inativo_com_sku_zerado_nao_aparece_na_visao()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        var nome = $"Produto inativo {Guid.NewGuid():N}";
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            context.Produto.Add(new Produto
            {
                Id = Guid.NewGuid(), Nome = nome, Marca = "Marca Teste",
                Categoria = "Calçado", Cor = "Azul", Ativo = false,
                Skus = new List<Sku>
                {
                    new() { Id = Guid.NewGuid(), Numeracao = "50", SaldoAtual = 0, Ativo = true }
                }
            });
            await context.SaveChangesAsync();
        }
        await LoginAsync(client, "lojista@squad.com");

        using var response = await client.GetAsync("/SaldosZerados");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain(nome, html);
        Assert.DoesNotContain(">50</span>", html);
    }

    [Fact]
    public async Task Lojista_recebe_mensagem_clara_quando_nao_ha_saldos_zerados()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            var skus = await context.Sku.ToListAsync();
            foreach (var sku in skus)
                sku.SaldoAtual = 1;
            await context.SaveChangesAsync();
        }
        await LoginAsync(client, "lojista@squad.com");

        using var response = await client.GetAsync("/SaldosZerados");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Nenhum produto possui numerações com saldo zerado no momento.", html);
    }

    [Theory]
    [InlineData("vendedor@squad.com", "/Account/AccessDenied")]
    [InlineData(null, "/Account/Login")]
    public async Task Vendedor_e_anonimo_nao_acessam_visao_nem_recebem_dados_do_estoque(
        string? email, string destino)
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = CreateClient(factory);
        var nome = await AddProductAsync(factory);
        if (email != null)
            await LoginAsync(client, email);

        using var response = await client.GetAsync("/SaldosZerados");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.StartsWith(destino, response.Headers.Location?.PathAndQuery);
        Assert.DoesNotContain(nome, body);
        Assert.DoesNotContain("Numerações sem saldo", body);
        Assert.DoesNotContain(">47</span>", body);

        Assert.NotNull(response.Headers.Location);
        using var destinoResponse = await client.GetAsync(response.Headers.Location);
        var destinoBody = await destinoResponse.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, destinoResponse.StatusCode);
        Assert.DoesNotContain(nome, destinoBody);
        Assert.DoesNotContain("Numerações sem saldo", destinoBody);
        Assert.DoesNotContain(">47</span>", destinoBody);
    }

    [Fact]
    public async Task Politicas_de_produtos_e_consulta_do_vendedor_permanecem_inalteradas()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var lojista = CreateClient(factory);
        using var vendedor = CreateClient(factory);
        await LoginAsync(lojista, "lojista@squad.com");
        await LoginAsync(vendedor, "vendedor@squad.com");

        using var produtos = await lojista.GetAsync("/Produtos");
        using var consulta = await vendedor.GetAsync("/Estoque/Consulta");
        using var produtosNegados = await vendedor.GetAsync("/Produtos");

        Assert.Equal(HttpStatusCode.OK, produtos.StatusCode);
        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
        Assert.Equal(HttpStatusCode.Found, produtosNegados.StatusCode);
        Assert.StartsWith("/Account/AccessDenied", produtosNegados.Headers.Location?.PathAndQuery);
    }

    private static HttpClient CreateClient(SquadEstoqueWebApplicationFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

    private static async Task<string> AddProductAsync(SquadEstoqueWebApplicationFactory factory)
    {
        var nome = $"Produto zerado {Guid.NewGuid():N}";
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        context.Produto.Add(new Produto
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Marca = "Marca Teste",
            Categoria = "Calçado",
            Cor = "Azul",
            Ativo = true,
            Skus = new List<Sku>
            {
                new() { Id = Guid.NewGuid(), Numeracao = "47", SaldoAtual = 0, Ativo = true, UltimaAtualizacaoSaldoEm = new DateTime(2026, 10, 29, 14, 30, 0, DateTimeKind.Utc) },
                new() { Id = Guid.NewGuid(), Numeracao = "46", SaldoAtual = 0, Ativo = true },
                new() { Id = Guid.NewGuid(), Numeracao = "48", SaldoAtual = 2, Ativo = true },
                new() { Id = Guid.NewGuid(), Numeracao = "49", SaldoAtual = 0, Ativo = false }
            }
        });
        await context.SaveChangesAsync();
        return nome;
    }

    private static async Task LoginAsync(HttpClient client, string email)
    {
        using var page = await client.GetAsync("/Account/Login");
        var match = Regex.Match(await page.Content.ReadAsStringAsync(),
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"[^>]*>",
            RegexOptions.IgnoreCase);
        Assert.True(match.Success, "Token antiforgery não encontrado.");

        using var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Senha"] = "123",
            ["__RequestVerificationToken"] = WebUtility.HtmlDecode(match.Groups[1].Value)
        });
        using var response = await client.PostAsync("/Account/Login", form);
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
    }
}
