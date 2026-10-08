using System.Data.Common;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SquadEstoque.Web.Controllers;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class VendaTransacionalTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Commit_persists_balance_and_traceable_movement_without_changing_history(bool administrativa)
    {
        var observer = new CommitObserver();
        await using var database = await SaleDatabase.CreateAsync(observer);
        observer.Enabled = true;
        var quantidade = administrativa ? 3 : 1;
        var inicio = DateTime.UtcNow;

        var result = await SellAsync(database, administrativa);

        Assert.Null(result.Erro);
        Assert.Equal(1, observer.CommitAttempts);
        Assert.Equal(5 - quantidade, observer.PendingBalance);
        Assert.Equal(1, observer.PendingExits);
        await using var persisted = database.NewContext();
        Assert.Equal(5 - quantidade, await persisted.Sku.Select(s => s.SaldoAtual).SingleAsync());
        var movement = await persisted.Movimentacao.SingleAsync(m => m.Tipo == TipoMovimentacao.SAIDA);
        Assert.NotEqual(Guid.Empty, movement.Id);
        Assert.Equal(database.Sku.Id, movement.SkuId);
        Assert.Equal(quantidade, movement.Quantidade);
        Assert.Equal(database.Usuario.Id, movement.UsuarioId);
        Assert.InRange(movement.CriadoEm, inicio, DateTime.UtcNow);
        Assert.Equal(administrativa ? "Venda administrativa" : null, movement.Motivo);
        Assert.Equal(2, await persisted.Movimentacao.CountAsync());
        await database.AssertHistoryUnchangedAsync(persisted);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Failed_insert_rolls_back_balance_and_preserves_history_and_context(bool administrativa)
    {
        await using var database = await SaleDatabase.CreateAsync();

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            SellAsync(database, administrativa, Guid.NewGuid()));

        await database.AssertRolledBackAsync();
        Assert.Null((await SellAsync(database, administrativa)).Erro);
        await using var persisted = database.NewContext();
        Assert.Equal(administrativa ? 2 : 4, await persisted.Sku.Select(s => s.SaldoAtual).SingleAsync());
        Assert.Equal(2, await persisted.Movimentacao.CountAsync());
        await database.AssertHistoryUnchangedAsync(persisted);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SQLite_commit_failure_rolls_back_saved_movement_and_balance(bool administrativa)
    {
        var observer = new CommitObserver();
        await using var database = await SaleDatabase.CreateAsync(observer);
        // A FK é verificada somente no COMMIT: SaveChanges chega a inserir a saída.
        observer.Enabled = true;
        observer.DeferForeignKeys = true;

        var error = await Assert.ThrowsAsync<SqliteException>(() =>
            SellAsync(database, administrativa, Guid.NewGuid()));

        Assert.Equal(19, error.SqliteErrorCode);
        Assert.Equal(1, observer.CommitAttempts);
        Assert.Equal(administrativa ? 2 : 4, observer.PendingBalance);
        Assert.Equal(1, observer.PendingExits);
        await database.AssertRolledBackAsync();
        observer.Enabled = false;
        observer.DeferForeignKeys = false;
        Assert.Null((await SellAsync(database, administrativa)).Erro);
        await using var persisted = database.NewContext();
        Assert.Equal(administrativa ? 2 : 4, await persisted.Sku.Select(s => s.SaldoAtual).SingleAsync());
        Assert.Equal(2, await persisted.Movimentacao.CountAsync());
        await database.AssertHistoryUnchangedAsync(persisted);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Rejected_decrement_never_inserts_movement_or_commits(bool administrativa)
    {
        var observer = new CommitObserver();
        await using var database = await SaleDatabase.CreateAsync(observer);
        await database.Context.Sku.ExecuteUpdateAsync(s => s.SetProperty(sku => sku.SaldoAtual, 0));
        observer.Enabled = true;

        var result = await SellAsync(database, administrativa);

        Assert.Contains("Saldo insuficiente", result.Erro);
        Assert.False(result.Conflito);
        Assert.Equal(0, observer.CommitAttempts);
        await using var persisted = database.NewContext();
        Assert.Equal(0, await persisted.Sku.Select(s => s.SaldoAtual).SingleAsync());
        Assert.Equal(1, await persisted.Movimentacao.CountAsync());
        await database.AssertHistoryUnchangedAsync(persisted);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Locked_SQLite_returns_safe_conflict_and_allows_retry(bool administrativa)
    {
        await using var database = await SaleDatabase.CreateAsync();
        await using var writer = new SqliteConnection(database.ConnectionString);
        await writer.OpenAsync();
        await using (var transaction = writer.BeginTransaction())
        {
            var result = await SellAsync(database, administrativa);

            Assert.True(result.Conflito);
            Assert.Equal("O estoque está sendo atualizado por outra operação. Tente novamente.", result.Erro);
            Assert.Null(result.Sku);
            await transaction.RollbackAsync();
        }

        await database.AssertRolledBackAsync();
        Assert.Null((await SellAsync(database, administrativa)).Erro);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Controllers_expose_safe_conflict_without_partial_sale(bool administrativa)
    {
        await using var database = await SaleDatabase.CreateAsync();
        await using var writer = new SqliteConnection(database.ConnectionString);
        await writer.OpenAsync();
        await using (var transaction = writer.BeginTransaction())
        {
            if (administrativa)
            {
                var controller = Authenticate(new MovimentacoesController(database.Context), database.Usuario.Id);
                Assert.IsType<ViewResult>(await controller.Saida(new MovimentacaoCreateViewModel
                {
                    SkuId = database.Sku.Id, Quantidade = 3
                }));
                Assert.Contains(controller.ModelState.Values.SelectMany(v => v.Errors), e =>
                    e.ErrorMessage == "O estoque está sendo atualizado por outra operação. Tente novamente.");
            }
            else
            {
                var controller = Authenticate(new EstoqueController(database.Context), database.Usuario.Id);
                var result = Assert.IsType<ConflictObjectResult>(
                    await controller.Vender(database.Sku.Id, database.Sku.ProdutoId));
                Assert.Equal(StatusCodes.Status409Conflict, result.StatusCode);
                var json = JsonSerializer.SerializeToElement(result.Value);
                Assert.Equal("O estoque está sendo atualizado por outra operação. Tente novamente.",
                    json.GetProperty("mensagem").GetString());
            }
            await transaction.RollbackAsync();
        }
        await database.AssertRolledBackAsync();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Controllers_return_safe_message_when_commit_fails(bool administrativa)
    {
        var observer = new CommitObserver();
        await using var database = await SaleDatabase.CreateAsync(observer);
        observer.Enabled = true;
        observer.DeferForeignKeys = true;
        var missingUsuario = Guid.NewGuid();
        if (administrativa)
        {
            var controller = Authenticate(new MovimentacoesController(database.Context), missingUsuario);
            Assert.IsType<ViewResult>(await controller.Saida(new MovimentacaoCreateViewModel
            {
                SkuId = database.Sku.Id, Quantidade = 3
            }));
            Assert.Contains(controller.ModelState.Values.SelectMany(v => v.Errors), e =>
                e.ErrorMessage == "Não foi possível registrar a saída de estoque. Operação cancelada. Tente novamente.");
        }
        else
        {
            var controller = Authenticate(new EstoqueController(database.Context), missingUsuario);
            var result = Assert.IsType<ObjectResult>(await controller.Vender(database.Sku.Id, database.Sku.ProdutoId));
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
            var json = JsonSerializer.SerializeToElement(result.Value);
            Assert.Equal("Não foi possível registrar a venda. Tente novamente.",
                json.GetProperty("mensagem").GetString());
        }
        Assert.Equal(1, observer.CommitAttempts);
        Assert.Equal(1, observer.PendingExits);
        await database.AssertRolledBackAsync();
    }

    private static Task<ResultadoSaida> SellAsync(SaleDatabase database, bool administrativa, Guid? usuarioId = null) =>
        administrativa
            ? database.Context.RegistrarSaidaAsync(database.Sku.Id, 3, usuarioId ?? database.Usuario.Id,
                "  Venda administrativa  ")
            : database.Context.RegistrarVendaRapidaAsync(database.Sku.Id, usuarioId ?? database.Usuario.Id);

    private static T Authenticate<T>(T controller, Guid usuarioId) where T : Controller
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
                    new Claim(ClaimTypes.Role, controller is EstoqueController ? "VENDEDOR" : "LOJISTA")
                }, "TestAuthentication"))
            }
        };
        return controller;
    }

    private sealed class CommitObserver : DbTransactionInterceptor
    {
        public bool Enabled { get; set; }
        public bool DeferForeignKeys { get; set; }
        public int CommitAttempts { get; private set; }
        public int PendingBalance { get; private set; }
        public int PendingExits { get; private set; }

        public override async ValueTask<DbTransaction> TransactionStartedAsync(
            DbConnection connection, TransactionEndEventData eventData, DbTransaction result,
            CancellationToken cancellationToken = default)
        {
            if (Enabled && DeferForeignKeys)
            {
                await using var command = connection.CreateCommand();
                command.Transaction = result;
                command.CommandText = "PRAGMA defer_foreign_keys = ON";
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            return result;
        }

        public override async ValueTask<InterceptionResult> TransactionCommittingAsync(
            DbTransaction transaction, TransactionEventData eventData, InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            if (Enabled)
            {
                CommitAttempts++;
                await using var command = transaction.Connection!.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = "SELECT SaldoAtual FROM Sku";
                PendingBalance = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
                command.CommandText = "SELECT COUNT(*) FROM Movimentacao WHERE Tipo = 1";
                PendingExits = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
            }
            return result;
        }
    }

    private sealed record HistorySnapshot(Guid Id, Guid SkuId, TipoMovimentacao Tipo, int Quantidade,
        Guid UsuarioId, DateTime CriadoEm, string? Motivo)
    {
        public static HistorySnapshot From(Movimentacao m) =>
            new(m.Id, m.SkuId, m.Tipo, m.Quantidade, m.UsuarioId, m.CriadoEm, m.Motivo);
    }

    private sealed class SaleDatabase : IAsyncDisposable
    {
        private readonly string _path = Path.Combine(Path.GetTempPath(), $"venda-transacional-{Guid.NewGuid():N}.db");
        private DbContextOptions<EstoqueContext> _options = null!;
        private HistorySnapshot _history = null!;
        public string ConnectionString => $"Data Source={_path};Pooling=False;Default Timeout=1";
        public EstoqueContext Context { get; private set; } = null!;
        public Sku Sku { get; private set; } = null!;
        public Usuario Usuario { get; private set; } = null!;
        public EstoqueContext NewContext() => new(_options);

        public static async Task<SaleDatabase> CreateAsync(CommitObserver? observer = null)
        {
            var database = new SaleDatabase();
            var options = new DbContextOptionsBuilder<EstoqueContext>().UseSqlite(database.ConnectionString);
            if (observer != null)
                options.AddInterceptors(observer);
            database._options = options.Options;
            database.Context = database.NewContext();
            await database.Context.Database.EnsureCreatedAsync();
            var produto = new Produto
            {
                Id = Guid.NewGuid(), Nome = "Tênis transacional", Marca = "Squad", Categoria = "Calçado", Cor = "Preto"
            };
            database.Sku = new Sku { Id = Guid.NewGuid(), ProdutoId = produto.Id, Numeracao = "38", SaldoAtual = 5 };
            database.Usuario = new Usuario
            {
                Id = Guid.NewGuid(), Nome = "Usuário da venda", Email = $"{Guid.NewGuid():N}@example.test",
                SenhaHash = "hash-teste", Perfil = PerfilUsuario.LOJISTA
            };
            var historico = new Movimentacao
            {
                Id = Guid.NewGuid(), SkuId = database.Sku.Id, Tipo = TipoMovimentacao.ENTRADA, Quantidade = 5,
                UsuarioId = database.Usuario.Id, CriadoEm = new DateTime(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                Motivo = "Entrada anterior à venda"
            };
            database.Context.AddRange(produto, database.Sku, database.Usuario, historico);
            await database.Context.SaveChangesAsync();
            database._history = HistorySnapshot.From(historico);
            return database;
        }

        public async Task AssertHistoryUnchangedAsync(EstoqueContext context)
        {
            var history = await context.Movimentacao.AsNoTracking().SingleAsync(m => m.Id == _history.Id);
            Assert.Equal(_history, HistorySnapshot.From(history));
        }

        public async Task AssertRolledBackAsync()
        {
            Assert.Equal(5, Sku.SaldoAtual);
            Assert.Null(Context.Database.CurrentTransaction);
            Assert.DoesNotContain(Context.ChangeTracker.Entries<Movimentacao>(), e => e.Entity.Id != _history.Id);
            await using var persisted = NewContext();
            Assert.Equal(5, await persisted.Sku.Select(s => s.SaldoAtual).SingleAsync());
            Assert.Equal(1, await persisted.Movimentacao.CountAsync());
            await AssertHistoryUnchangedAsync(persisted);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            File.Delete(_path);
            File.Delete(_path + "-wal");
            File.Delete(_path + "-shm");
        }
    }
}
