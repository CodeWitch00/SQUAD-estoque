using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Controllers;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

// As constraints de unicidade e saldo são protegidas pelo EstoqueContext.
// Entrada, saída, saldo insuficiente e ajuste ainda são regras do controller;
// estes testes caracterizam o comportamento no ponto em que ele existe hoje.
public sealed class EstoqueDomainPersistenceTests
{
    [Fact]
    public void Produto_requires_identification_fields()
    {
        var produto = new Produto();
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            produto,
            new ValidationContext(produto),
            validationResults,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(Produto.Nome)));
        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(Produto.Marca)));
        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(Produto.Categoria)));
        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(Produto.Cor)));
    }

    [Fact]
    public async Task Sku_with_same_product_and_numeracao_cannot_be_persisted_twice()
    {
        await using var database = await TestDatabase.CreateAsync();
        var produto = CreateProduto();
        database.Context.Add(produto);
        database.Context.Sku.AddRange(
            CreateSku(produto.Id, "38", 1),
            CreateSku(produto.Id, "38", 2));

        await Assert.ThrowsAsync<DbUpdateException>(() => database.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task Sku_with_negative_balance_cannot_be_persisted()
    {
        await using var database = await TestDatabase.CreateAsync();
        var produto = CreateProduto();
        database.Context.Add(produto);
        database.Context.Sku.Add(CreateSku(produto.Id, "39", -1));

        await Assert.ThrowsAsync<DbUpdateException>(() => database.Context.SaveChangesAsync());
    }

    [Fact]
    public async Task Entrada_registers_movement_and_increases_balance()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, initialBalance: 3);
        var controller = CreateController(database.Context, data.Usuario);

        var result = await controller.Entrada(new MovimentacaoCreateViewModel
        {
            SkuId = data.Sku.Id,
            Quantidade = 4,
            Motivo = "Reposição"
        });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(7, data.Sku.SaldoAtual);
        var movimentacao = await database.Context.Movimentacao.SingleAsync();
        Assert.Equal(TipoMovimentacao.ENTRADA, movimentacao.Tipo);
        Assert.Equal(4, movimentacao.Quantidade);
    }

    [Fact]
    public async Task Saida_registers_movement_and_reduces_balance()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, initialBalance: 5);
        var controller = CreateController(database.Context, data.Usuario);

        var result = await controller.Saida(new MovimentacaoCreateViewModel
        {
            SkuId = data.Sku.Id,
            Quantidade = 2
        });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(3, data.Sku.SaldoAtual);
        var movimentacao = await database.Context.Movimentacao.SingleAsync();
        Assert.Equal(TipoMovimentacao.SAIDA, movimentacao.Tipo);
        Assert.Equal(2, movimentacao.Quantidade);
    }

    [Fact]
    public async Task Saida_with_insufficient_balance_is_rejected_by_controller()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, initialBalance: 1);
        var controller = CreateController(database.Context, data.Usuario);

        var result = await controller.Saida(new MovimentacaoCreateViewModel
        {
            SkuId = data.Sku.Id,
            Quantidade = 2
        });

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal(1, data.Sku.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Ajuste_registers_movement_and_updates_balance()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, initialBalance: 2);
        var controller = CreateController(database.Context, data.Usuario);

        var result = await controller.Ajuste(new AjusteEstoqueViewModel
        {
            SkuId = data.Sku.Id,
            NovoSaldoApurado = 6,
            Motivo = "Contagem física"
        });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(6, data.Sku.SaldoAtual);
        var movimentacao = await database.Context.Movimentacao.SingleAsync();
        Assert.Equal(TipoMovimentacao.AJUSTE, movimentacao.Tipo);
        Assert.Equal(4, movimentacao.Quantidade);
        Assert.Equal("Contagem física", movimentacao.Motivo);
    }

    [Fact]
    public async Task Ajuste_without_reason_is_rejected_by_controller()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, initialBalance: 2);
        var controller = CreateController(database.Context, data.Usuario);

        var result = await controller.Ajuste(new AjusteEstoqueViewModel
        {
            SkuId = data.Sku.Id,
            NovoSaldoApurado = 6,
            Motivo = string.Empty
        });

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal(2, data.Sku.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Ruptura_can_be_persisted_without_changing_balance()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, initialBalance: 3);
        database.Context.Ruptura.Add(new Ruptura
        {
            Id = Guid.NewGuid(),
            SkuId = data.Sku.Id,
            UsuarioId = data.Usuario.Id,
            CriadoEm = DateTime.UtcNow
        });

        await database.Context.SaveChangesAsync();

        Assert.Single(await database.Context.Ruptura.ToListAsync());
        Assert.Equal(3, data.Sku.SaldoAtual);
    }

    private static MovimentacoesController CreateController(EstoqueContext context, Usuario usuario)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuthentication");

        return new MovimentacoesController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }

    private static async Task<TestData> SeedAsync(EstoqueContext context, int initialBalance)
    {
        var produto = CreateProduto();
        var sku = CreateSku(produto.Id, "38", initialBalance);
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Usuário de Teste",
            Email = $"{Guid.NewGuid():N}@example.test",
            SenhaHash = "hash-isolado-de-teste",
            Perfil = PerfilUsuario.LOJISTA
        };

        context.AddRange(produto, sku, usuario);
        await context.SaveChangesAsync();
        return new TestData(sku, usuario);
    }

    private static Produto CreateProduto()
    {
        return new Produto
        {
            Id = Guid.NewGuid(),
            Nome = "Tênis de Teste",
            Marca = "Marca Teste",
            Categoria = "Esportivo",
            Cor = "Preto"
        };
    }

    private static Sku CreateSku(Guid produtoId, string numeracao, int saldoAtual)
    {
        return new Sku
        {
            Id = Guid.NewGuid(),
            ProdutoId = produtoId,
            Numeracao = numeracao,
            SaldoAtual = saldoAtual
        };
    }

    private sealed record TestData(Sku Sku, Usuario Usuario);

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private TestDatabase(SqliteConnection connection, EstoqueContext context)
        {
            _connection = connection;
            Context = context;
        }

        public EstoqueContext Context { get; }

        public static async Task<TestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
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
