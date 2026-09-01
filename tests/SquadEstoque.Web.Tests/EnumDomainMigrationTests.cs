using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SquadEstoque.Web.Data;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class EnumDomainMigrationTests
{
    private const string PreviousMigration = "20260817120442_InitialSquadSchema";
    private const string EnumMigration = "20260901000759_AddEnumDomainConstraints";

    [Fact]
    public async Task Enum_migration_accepts_documented_values_rejects_others_and_reverts()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<EstoqueContext>()
            .UseSqlite(connection)
            .Options;
        await using var context = new EstoqueContext(options);
        var migrator = context.Database.GetService<IMigrator>();

        await migrator.MigrateAsync(EnumMigration);
        var ids = await InsertRequiredDataAsync(context);

        await InsertUsuarioAsync(context, Guid.NewGuid(), "vendedor@enum.test", 0);
        await InsertUsuarioAsync(context, Guid.NewGuid(), "lojista@enum.test", 1);
        await Assert.ThrowsAsync<SqliteException>(() =>
            InsertUsuarioAsync(context, Guid.NewGuid(), "invalido@enum.test", 2));

        await InsertMovimentacaoAsync(context, ids.SkuId, ids.UsuarioId, 0);
        await InsertMovimentacaoAsync(context, ids.SkuId, ids.UsuarioId, 1);
        await InsertMovimentacaoAsync(context, ids.SkuId, ids.UsuarioId, 2);
        await Assert.ThrowsAsync<SqliteException>(() =>
            InsertMovimentacaoAsync(context, ids.SkuId, ids.UsuarioId, 3));

        await migrator.MigrateAsync(PreviousMigration);

        // Após o Down, os CHECKs não existem mais e os mesmos valores são aceitos.
        await InsertUsuarioAsync(context, Guid.NewGuid(), "apos-rollback@enum.test", 2);
        await InsertMovimentacaoAsync(context, ids.SkuId, ids.UsuarioId, 3);
    }

    private static async Task<(Guid SkuId, Guid UsuarioId)> InsertRequiredDataAsync(EstoqueContext context)
    {
        var produtoId = Guid.NewGuid();
        var skuId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        await context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO Produto (Id, Nome, Marca, Categoria, Cor, Ativo)
            VALUES ({produtoId}, 'Produto Enum', 'Marca', 'Categoria', 'Cor', 1)
            """);
        await InsertUsuarioAsync(context, usuarioId, "base@enum.test", 0);
        await context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO Sku (Id, ProdutoId, Numeracao, SaldoAtual, Ativo)
            VALUES ({skuId}, {produtoId}, '38', 1, 1)
            """);

        return (skuId, usuarioId);
    }

    private static Task<int> InsertUsuarioAsync(
        EstoqueContext context,
        Guid id,
        string email,
        int perfil) =>
        context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO Usuario (Id, Nome, Email, SenhaHash, Perfil)
            VALUES ({id}, 'Usuário Enum', {email}, 'hash', {perfil})
            """);

    private static Task<int> InsertMovimentacaoAsync(
        EstoqueContext context,
        Guid skuId,
        Guid usuarioId,
        int tipo) =>
        context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO Movimentacao (Id, SkuId, Tipo, Quantidade, UsuarioId, CriadoEm, Motivo)
            VALUES ({Guid.NewGuid()}, {skuId}, {tipo}, 1, {usuarioId}, {DateTime.UtcNow}, NULL)
            """);
}
