using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class ConsultaIndexesMigrationTests
{
    [Fact]
    public async Task Migration_cria_indices_preserva_dados_e_reverte()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<EstoqueContext>().UseSqlite(connection).Options;
        await using var context = new EstoqueContext(options);
        var migrator = context.GetService<IMigrator>();
        const string anterior = "20261026220326_AddSkuUltimaAtualizacaoSaldo";

        await migrator.MigrateAsync(anterior);
        var produto = new Produto
        {
            Id = Guid.NewGuid(), Nome = "Modelo", Marca = "Marca",
            Categoria = "Calçado", Cor = "Azul", Ativo = true
        };
        var sku = new Sku
        {
            Id = Guid.NewGuid(), ProdutoId = produto.Id,
            Numeracao = "39", SaldoAtual = 0, Ativo = true
        };
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(), Nome = "Lojista", Email = "indice@example.test",
            SenhaHash = "hash", Perfil = PerfilUsuario.LOJISTA
        };
        context.AddRange(produto, sku, usuario);
        context.Ruptura.Add(new Ruptura
        {
            Id = Guid.NewGuid(), SkuId = sku.Id, UsuarioId = usuario.Id,
            CriadoEm = new DateTime(2026, 10, 29, 12, 0, 0, DateTimeKind.Utc)
        });
        await context.SaveChangesAsync();

        await migrator.MigrateAsync();
        Assert.Equal(1, await IndexCountAsync(context, "IX_Produto_Marca"));
        Assert.Equal(1, await IndexCountAsync(context, "IX_Produto_Categoria"));
        Assert.Equal(1, await IndexCountAsync(context, "IX_Sku_ProdutoId_SaldoZero"));
        Assert.Equal(1, await IndexCountAsync(context, "IX_Ruptura_SkuId_CriadoEm"));
        Assert.Equal(0, await IndexCountAsync(context, "IX_Ruptura_SkuId"));
        Assert.Contains("WHERE SaldoAtual = 0", await context.Database.SqlQueryRaw<string>(
            "SELECT sql AS Value FROM sqlite_master WHERE name = 'IX_Sku_ProdutoId_SaldoZero'").SingleAsync());
        await AssertDadosAsync(context);

        await migrator.MigrateAsync(anterior);
        foreach (var nome in new[] { "IX_Produto_Marca", "IX_Produto_Categoria",
            "IX_Sku_ProdutoId_SaldoZero", "IX_Ruptura_SkuId_CriadoEm" })
            Assert.Equal(0, await IndexCountAsync(context, nome));
        Assert.Equal(1, await IndexCountAsync(context, "IX_Ruptura_SkuId"));
        await AssertDadosAsync(context);

        await migrator.MigrateAsync();
        Assert.Equal(1, await IndexCountAsync(context, "IX_Ruptura_SkuId_CriadoEm"));
        await AssertDadosAsync(context);
    }

    private static Task<int> IndexCountAsync(EstoqueContext context, string name) =>
        context.Database.SqlQueryRaw<int>("SELECT COUNT(*) AS Value FROM sqlite_master WHERE type = 'index' AND name = {0}", name).SingleAsync();

    private static async Task AssertDadosAsync(EstoqueContext context)
    {
        Assert.Equal(1, await context.Produto.CountAsync());
        Assert.Equal(1, await context.Sku.CountAsync());
        Assert.Equal(1, await context.Ruptura.CountAsync());
        Assert.Equal(0, await context.Sku.Select(s => s.SaldoAtual).SingleAsync());
    }
}
