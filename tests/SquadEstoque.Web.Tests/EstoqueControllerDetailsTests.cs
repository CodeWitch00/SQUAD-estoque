using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Controllers;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class EstoqueControllerDetailsTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly EstoqueContext _context;

    public EstoqueControllerDetailsTests()
    {
        _connection.Open();
        _context = new EstoqueContext(new DbContextOptionsBuilder<EstoqueContext>().UseSqlite(_connection).Options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task Detalhes_returns_active_product_with_complete_sorted_grade()
    {
        var produto = AddProduto(true, ("40", 3), ("38", 0), ("39", 1));

        var result = await Controller().Detalhes(produto.Id);

        var model = Assert.IsType<EstoqueProdutoDetalhesViewModel>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(new[] { "38", "39", "40" }, model.Skus.Select(s => s.Numeracao));
        Assert.Equal(new[] { 0, 1, 3 }, model.Skus.Select(s => s.SaldoAtual));
    }

    [Fact]
    public async Task Detalhes_returns_not_found_for_nonexistent_product() =>
        Assert.IsType<NotFoundResult>(await Controller().Detalhes(Guid.NewGuid()));

    [Fact]
    public async Task Detalhes_returns_not_found_for_inactive_product()
    {
        var produto = AddProduto(false, ("39", 2));

        Assert.IsType<NotFoundResult>(await Controller().Detalhes(produto.Id));
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private EstoqueController Controller() => new(_context);

    private Produto AddProduto(bool ativo, params (string numero, int saldo)[] grade)
    {
        var produto = new Produto { Id = Guid.NewGuid(), Nome = "Tênis", Marca = "Squad", Categoria = "Calçado", Cor = "Preto", Ativo = ativo };
        foreach (var (numero, saldo) in grade)
            produto.Skus.Add(new Sku { Id = Guid.NewGuid(), ProdutoId = produto.Id, Numeracao = numero, SaldoAtual = saldo, Ativo = true });
        _context.Produto.Add(produto);
        _context.SaveChanges();
        return produto;
    }
}
