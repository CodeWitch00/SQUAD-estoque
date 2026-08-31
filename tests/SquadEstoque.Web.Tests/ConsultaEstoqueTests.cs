using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class ConsultaEstoqueTests : IClassFixture<SquadEstoqueWebApplicationFactory>
{
    private readonly SquadEstoqueWebApplicationFactory _factory;

    public ConsultaEstoqueTests(SquadEstoqueWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Anonymous_user_is_redirected_to_login()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/Estoque/Consulta");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal(
            "/Account/Login?ReturnUrl=%2FEstoque%2FConsulta",
            response.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task Lojista_is_redirected_to_access_denied()
    {
        using var client = CreateClient();
        await LoginAsync(client, "lojista@squad.com");

        var response = await client.GetAsync("/Estoque/Consulta");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal(
            "/Account/AccessDenied?ReturnUrl=%2FEstoque%2FConsulta",
            response.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task Vendedor_sees_accessible_initial_state()
    {
        using var client = CreateClient();
        await LoginAsync(client, "vendedor@squad.com");

        var response = await client.GetAsync("/Estoque/Consulta");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Consulta rápida", html);
        Assert.Contains("<label", html);
        Assert.Contains("for=\"Termo\"", html);
        Assert.Contains("role=\"search\"", html);
        Assert.Contains("Pronto para consultar", html);
    }

    [Fact]
    public async Task Search_with_one_character_shows_validation_error_without_query_results()
    {
        var marker = $"Z{Guid.NewGuid():N}";
        await AddProductsAsync(CreateProduct($"{marker} produto"));
        using var client = CreateClient();
        await LoginAsync(client, "vendedor@squad.com");

        var response = await client.GetAsync("/Estoque/Consulta?termo=Z");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Digite pelo menos 2 caracteres para buscar.", html);
        Assert.DoesNotContain(marker, html);
    }

    [Fact]
    public async Task Search_finds_active_products_by_supported_fields_and_orders_by_name()
    {
        var marker = Guid.NewGuid().ToString("N")[..10];
        var first = CreateProduct("Alpha " + marker, marca: "Marca comum");
        var second = CreateProduct("Beta " + marker, categoria: marker);
        var third = CreateProduct("Gama " + marker, cor: marker);
        var inactive = CreateProduct("Inativo " + marker, ativo: false);
        await AddProductsAsync(second, inactive, third, first);
        using var client = CreateClient();
        await LoginAsync(client, "vendedor@squad.com");

        var response = await client.GetAsync($"/Estoque/Consulta?termo={marker.ToUpperInvariant()}");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("3 produtos encontrados", html);
        Assert.Contains(first.Nome, html);
        Assert.Contains(second.Nome, html);
        Assert.Contains(third.Nome, html);
        Assert.DoesNotContain(inactive.Nome, html);
        Assert.True(html.IndexOf(first.Nome, StringComparison.Ordinal) <
                    html.IndexOf(second.Nome, StringComparison.Ordinal));
        Assert.True(html.IndexOf(second.Nome, StringComparison.Ordinal) <
                    html.IndexOf(third.Nome, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Search_without_matches_shows_empty_state()
    {
        using var client = CreateClient();
        await LoginAsync(client, "vendedor@squad.com");

        var response = await client.GetAsync($"/Estoque/Consulta?termo=ausente{Guid.NewGuid():N}");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Produto não encontrado.", html);
        Assert.Contains("Revise o termo ou faça uma nova busca.", html);
    }

    [Fact]
    public async Task Vendedor_can_select_one_of_the_current_results()
    {
        var marker = Guid.NewGuid().ToString("N")[..10];
        var product = CreateProduct("Selecionável " + marker);
        await AddProductsAsync(product);
        using var client = CreateClient();
        await LoginAsync(client, "vendedor@squad.com");

        var response = await client.GetAsync(
            $"/Estoque/Consulta?termo={marker}&produtoId={product.Id}");
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"{product.Nome} selecionado.", html);
        Assert.Contains("aria-current=\"true\"", html);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    private async Task AddProductsAsync(params Produto[] products)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        context.Produto.AddRange(products);
        await context.SaveChangesAsync();
    }

    private static Produto CreateProduct(
        string nome,
        string marca = "Marca teste",
        string categoria = "Categoria teste",
        string cor = "Cor teste",
        bool ativo = true)
    {
        return new Produto
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Marca = marca,
            Categoria = categoria,
            Cor = cor,
            Ativo = ativo
        };
    }

    private static async Task LoginAsync(HttpClient client, string email)
    {
        var loginPage = await client.GetAsync("/Account/Login");
        loginPage.EnsureSuccessStatusCode();
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiforgeryToken(html);
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Senha"] = "123",
            ["__RequestVerificationToken"] = token
        });

        var response = await client.PostAsync("/Account/Login", content);
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
    }

    private static string ExtractAntiforgeryToken(string html)
    {
        var match = Regex.Match(
            html,
            "<input[^>]*name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"[^>]*>",
            RegexOptions.IgnoreCase);

        Assert.True(match.Success, "Token antiforgery não encontrado no formulário de login.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }
}
