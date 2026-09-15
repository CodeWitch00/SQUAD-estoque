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
// A saída compartilhada e os controllers são verificados contra SQLite real.
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

        var before = DateTime.UtcNow;
        var result = await controller.Entrada(new MovimentacaoCreateViewModel
        {
            SkuId = data.Sku.Id,
            Quantidade = 4,
            Motivo = "Reposição"
        });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(7, data.Sku.SaldoAtual);
        var movimentacao = await database.Context.Movimentacao.SingleAsync();
        Assert.NotEqual(Guid.Empty, movimentacao.Id);
        Assert.Equal(data.Sku.Id, movimentacao.SkuId);
        Assert.Equal(TipoMovimentacao.ENTRADA, movimentacao.Tipo);
        Assert.Equal(4, movimentacao.Quantidade);
        Assert.Equal(data.Usuario.Id, movimentacao.UsuarioId);
        Assert.InRange(movimentacao.CriadoEm, before, DateTime.UtcNow);
        Assert.Equal("Reposição", movimentacao.Motivo);
    }

    [Fact]
    public async Task Saida_registers_movement_and_reduces_balance()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, initialBalance: 5);
        var controller = CreateController(database.Context, data.Usuario);

        var before = DateTime.UtcNow;
        var result = await controller.Saida(new MovimentacaoCreateViewModel
        {
            SkuId = data.Sku.Id,
            Quantidade = 2
        });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(3, data.Sku.SaldoAtual);
        var movimentacao = await database.Context.Movimentacao.SingleAsync();
        Assert.NotEqual(Guid.Empty, movimentacao.Id);
        Assert.Equal(data.Sku.Id, movimentacao.SkuId);
        Assert.Equal(TipoMovimentacao.SAIDA, movimentacao.Tipo);
        Assert.Equal(2, movimentacao.Quantidade);
        Assert.Equal(data.Usuario.Id, movimentacao.UsuarioId);
        Assert.InRange(movimentacao.CriadoEm, before, DateTime.UtcNow);
        Assert.Null(movimentacao.Motivo);
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

        var before = DateTime.UtcNow;
        var result = await controller.Ajuste(new AjusteEstoqueViewModel
        {
            SkuId = data.Sku.Id,
            NovoSaldoApurado = 6,
            Motivo = "Contagem física"
        });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(6, data.Sku.SaldoAtual);
        var movimentacao = await database.Context.Movimentacao.SingleAsync();
        Assert.NotEqual(Guid.Empty, movimentacao.Id);
        Assert.Equal(data.Sku.Id, movimentacao.SkuId);
        Assert.Equal(TipoMovimentacao.AJUSTE, movimentacao.Tipo);
        Assert.Equal(4, movimentacao.Quantidade);
        Assert.Equal(data.Usuario.Id, movimentacao.UsuarioId);
        Assert.InRange(movimentacao.CriadoEm, before, DateTime.UtcNow);
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

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task Venda_rapida_removes_exactly_one_pair_from_selected_sku(int saldo)
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, saldo);
        data.Usuario.Perfil = PerfilUsuario.VENDEDOR;
        var outroSku = CreateSku(data.Sku.ProdutoId, "40", 7);
        database.Context.Add(outroSku);
        await database.Context.SaveChangesAsync();
        var antes = DateTime.UtcNow;

        var resultado = await database.Context.RegistrarVendaRapidaAsync(data.Sku.Id, data.Usuario.Id);

        Assert.Null(resultado.Erro);
        database.Context.ChangeTracker.Clear();
        Assert.Equal(saldo - 1, (await database.Context.Sku.FindAsync(data.Sku.Id))!.SaldoAtual);
        Assert.Equal(7, (await database.Context.Sku.FindAsync(outroSku.Id))!.SaldoAtual);
        var movimento = await database.Context.Movimentacao.SingleAsync();
        Assert.Equal(data.Sku.Id, movimento.SkuId);
        Assert.Equal(data.Usuario.Id, movimento.UsuarioId);
        Assert.Equal(TipoMovimentacao.SAIDA, movimento.Tipo);
        Assert.Equal(1, movimento.Quantidade);
        Assert.InRange(movimento.CriadoEm, antes, DateTime.UtcNow);
        Assert.Empty(await database.Context.Ruptura.ToListAsync());
    }

    [Fact]
    public async Task Venda_rapida_rejects_zero_balance_without_movement()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, 0);
        var resultado = await database.Context.RegistrarVendaRapidaAsync(data.Sku.Id, data.Usuario.Id);
        Assert.Contains("Saldo insuficiente", resultado.Erro);
        database.Context.ChangeTracker.Clear();
        Assert.Equal(0, (await database.Context.Sku.FindAsync(data.Sku.Id))!.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Venda_rapida_rejects_missing_sku()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, 3);
        var resultado = await database.Context.RegistrarVendaRapidaAsync(Guid.NewGuid(), data.Usuario.Id);
        Assert.Contains("não foi encontrado", resultado.Erro);
        Assert.Equal(3, data.Sku.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Shared_saida_rejects_nonpositive_quantity(int quantidade)
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, 3);
        var resultado = await database.Context.RegistrarSaidaAsync(data.Sku.Id, quantidade, data.Usuario.Id);
        Assert.Equal("Quantidade", resultado.Campo);
        Assert.NotNull(resultado.Erro);
        Assert.Equal(3, data.Sku.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Failed_movement_rolls_back_balance_and_allows_next_operation()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, 3);
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            database.Context.RegistrarVendaRapidaAsync(data.Sku.Id, Guid.NewGuid()));
        Assert.Equal(3, data.Sku.SaldoAtual);
        Assert.Equal(3, await database.Context.Sku.AsNoTracking()
            .Where(s => s.Id == data.Sku.Id).Select(s => s.SaldoAtual).SingleAsync());
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());

        var resultado = await database.Context.RegistrarVendaRapidaAsync(data.Sku.Id, data.Usuario.Id);
        Assert.Null(resultado.Erro);
        Assert.Equal(2, data.Sku.SaldoAtual);
        Assert.Single(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Shared_saida_refreshes_previously_tracked_balance()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, 3);
        await database.Context.Sku.Where(s => s.Id == data.Sku.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.SaldoAtual, 0));
        var resultado = await database.Context.RegistrarVendaRapidaAsync(data.Sku.Id, data.Usuario.Id);
        Assert.NotNull(resultado.Erro);
        Assert.Equal(0, data.Sku.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
    }

    [Fact]
    public async Task Administrative_saida_preserves_quantity_reason_user_and_redirect()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, 5);
        var controller = CreateController(database.Context, data.Usuario);
        var result = await controller.Saida(new MovimentacaoCreateViewModel
        {
            SkuId = data.Sku.Id, Quantidade = 3, Motivo = "  Venda administrativa  "
        });
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("Produtos", redirect.ControllerName);
        Assert.Equal(data.Sku.ProdutoId, redirect.RouteValues!["id"]);
        database.Context.ChangeTracker.Clear();
        Assert.Equal(2, (await database.Context.Sku.FindAsync(data.Sku.Id))!.SaldoAtual);
        var movimento = await database.Context.Movimentacao.SingleAsync();
        Assert.Equal(3, movimento.Quantidade);
        Assert.Equal(data.Usuario.Id, movimento.UsuarioId);
        Assert.Equal("Venda administrativa", movimento.Motivo);
    }

    [Fact]
    public async Task Saida_without_authenticated_identifier_does_not_change_stock()
    {
        await using var database = await TestDatabase.CreateAsync();
        var data = await SeedAsync(database.Context, 3);
        var controller = CreateController(database.Context, data.Usuario);
        controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        var result = await controller.Saida(new MovimentacaoCreateViewModel
        {
            SkuId = data.Sku.Id, Quantidade = 1
        });
        Assert.IsType<ChallengeResult>(result);
        Assert.Equal(3, data.Sku.SaldoAtual);
        Assert.Empty(await database.Context.Movimentacao.ToListAsync());
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
