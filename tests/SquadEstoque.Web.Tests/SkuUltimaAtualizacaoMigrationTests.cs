using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class SkuUltimaAtualizacaoMigrationTests
{
    [Fact]
    public async Task Migration_preserva_sku_existente_inicializa_null_e_reverte()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<EstoqueContext>().UseSqlite(connection).Options;
        await using var context = new EstoqueContext(options);
        var migrator = context.GetService<IMigrator>();
        const string anterior = "20261009200729_AddEntradaFiscalEImagemProduto";
        await migrator.MigrateAsync(anterior);
        var produtoId = Guid.NewGuid();
        var skuId = Guid.NewGuid();
        await context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO Produto (Id, Nome, Marca, Categoria, Cor, Ativo) VALUES ({produtoId}, 'Teste', 'Marca', 'Categoria', 'Preto', 1)");
        await context.Database.ExecuteSqlInterpolatedAsync($"INSERT INTO Sku (Id, ProdutoId, Numeracao, SaldoAtual, Ativo) VALUES ({skuId}, {produtoId}, '38', 7, 1)");
        await migrator.MigrateAsync();
        var sku = await context.Sku.AsNoTracking().SingleAsync();
        Assert.Equal(skuId, sku.Id);
        Assert.Equal(7, sku.SaldoAtual);
        Assert.Null(sku.UltimaAtualizacaoSaldoEm);
        var instante = new DateTime(2026, 10, 26, 12, 0, 0, DateTimeKind.Utc);
        await context.Sku.ExecuteUpdateAsync(s => s.SetProperty(x => x.UltimaAtualizacaoSaldoEm, instante));
        sku = await context.Sku.AsNoTracking().SingleAsync();
        Assert.Equal(instante, sku.UltimaAtualizacaoSaldoEm);
        Assert.Equal(DateTimeKind.Utc, sku.UltimaAtualizacaoSaldoEm!.Value.Kind);
        await migrator.MigrateAsync(anterior);
        Assert.Equal(7, await context.Database.SqlQueryRaw<int>("SELECT SaldoAtual AS Value FROM Sku").SingleAsync());
        await migrator.MigrateAsync();
        Assert.Null((await context.Sku.AsNoTracking().SingleAsync()).UltimaAtualizacaoSaldoEm);
    }
}
