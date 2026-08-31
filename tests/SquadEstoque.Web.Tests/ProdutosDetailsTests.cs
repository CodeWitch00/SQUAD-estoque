using Microsoft.AspNetCore.Mvc;

using Microsoft.Data.Sqlite;

using Microsoft.EntityFrameworkCore;

using SquadEstoque.Web.Controllers;

using SquadEstoque.Web.Data;

using SquadEstoque.Web.Models;

using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class ProdutosDetailsTests

{

    [Fact]

    public async Task Details_returns_product_with_complete_grade_when_product_is_active()

    {

        await using var database = await TestDatabase.CreateAsync();

        var produto = CreateProduto();

        database.Context.Produto.Add(produto);

        database.Context.Sku.AddRange(

            CreateSku(produto.Id, "40", 5),

            CreateSku(produto.Id, "38", 3),

            CreateSku(produto.Id, "39", 2));

        await database.Context.SaveChangesAsync();

        var controller = new ProdutosController(database.Context);

        var result = await controller.Details(produto.Id);

        var viewResult = Assert.IsType<ViewResult>(result);

        var produtoRetornado = Assert.IsType<Produto>(viewResult.Model);

        Assert.Equal(produto.Id, produtoRetornado.Id);

        Assert.Equal("Tênis de Teste", produtoRetornado.Nome);

        Assert.True(produtoRetornado.Ativo);

        Assert.Equal(3, produtoRetornado.Skus.Count);

        Assert.Contains(produtoRetornado.Skus, sku => sku.Numeracao == "38");

        Assert.Contains(produtoRetornado.Skus, sku => sku.Numeracao == "39");

        Assert.Contains(produtoRetornado.Skus, sku => sku.Numeracao == "40");

    }

    [Fact]

    public async Task Details_returns_not_found_when_product_does_not_exist()

    {

        await using var database = await TestDatabase.CreateAsync();

        var controller = new ProdutosController(database.Context);

        var result = await controller.Details(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);

    }

    [Fact]

    public async Task Details_returns_not_found_when_product_is_inactive()

    {

        await using var database = await TestDatabase.CreateAsync();

        var produto = CreateProduto();

        produto.Ativo = false;

        database.Context.Produto.Add(produto);

        await database.Context.SaveChangesAsync();

        var controller = new ProdutosController(database.Context);

        var result = await controller.Details(produto.Id);

        Assert.IsType<NotFoundResult>(result);

    }

    private static Produto CreateProduto()

    {

        return new Produto

        {

            Id = Guid.NewGuid(),

            Nome = "Tênis de Teste",

            Marca = "Marca Teste",

            Categoria = "Esportivo",

            Cor = "Preto",

            Ativo = true

        };

    }

    private static Sku CreateSku(Guid produtoId, string numeracao, int saldoAtual)

    {

        return new Sku

        {

            Id = Guid.NewGuid(),

            ProdutoId = produtoId,

            Numeracao = numeracao,

            SaldoAtual = saldoAtual,

            Ativo = true

        };

    }

    private sealed class TestDatabase : IAsyncDisposable

    {

        private readonly SqliteConnection _connection;

        private TestDatabase(

            SqliteConnection connection,

            EstoqueContext context)

        {

            _connection = connection;

            Context = context;

        }

        public EstoqueContext Context { get; }

        public static async Task<TestDatabase> CreateAsync()

        {

            var connection = new SqliteConnection(

                "Data Source=:memory:");

            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<EstoqueContext>()

                .UseSqlite(connection)

                .Options;

            var context = new EstoqueContext(options);

            await context.Database.EnsureCreatedAsync();

            return new TestDatabase(connection, context);

        }

        public async ValueTask DisposeAsync()

        {

            await Context.DisposeAsync();

            await _connection.DisposeAsync();

        }

    }

}
 
