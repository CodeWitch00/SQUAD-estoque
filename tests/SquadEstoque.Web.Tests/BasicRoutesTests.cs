using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class BasicRoutesTests : IClassFixture<SquadEstoqueWebApplicationFactory>
{
    private readonly SquadEstoqueWebApplicationFactory _factory;

    public BasicRoutesTests(SquadEstoqueWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Application_starts_without_exception()
    {
        using var client = CreateClient();

        Assert.NotNull(client);
    }

    [Fact]
    public async Task Home_returns_ok()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_returns_ok()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/Account/Login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Produtos_without_authentication_redirects_to_login()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/Produtos");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/Account/Login?ReturnUrl=%2FProdutos", response.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task Movimentacoes_without_authentication_redirects_to_login()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/Movimentacoes");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/Account/Login?ReturnUrl=%2FMovimentacoes", response.Headers.Location?.PathAndQuery);
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }
}
