using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Data;
using Xunit;

namespace SquadEstoque.Web.Tests;

// S4-DB-004 — Cenário A: banco vazio (cadeia real de migrations, sem EnsureCreated).
public sealed class EstoqueMigrationsBancoVazioTests
{
    [Fact]
    public async Task Banco_vazio_inicializa_com_a_cadeia_completa_de_migrations()
    {
        var path = EstoqueMigrationsTestSupport.CreateDatabasePath();

        try
        {
            var options = EstoqueMigrationsTestSupport.CreateOptions(path);
            await using var context = new EstoqueContext(options);

            // 1. Inicialização do zero até a última migration versionada.
            await context.Database.MigrateAsync();

            await EstoqueMigrationsTestSupport.AssertHistoryAsync(
                context, EstoqueMigrationsTestSupport.MigrationsEsperadas);
            await EstoqueMigrationsTestSupport.AssertSchemaAsync(context);
            await EstoqueMigrationsTestSupport.AssertIntegrityAsync(context);
            await EstoqueMigrationsTestSupport.AssertEscritaLeituraAsync(context);
            await EstoqueMigrationsTestSupport.AssertConstraintsAsync(context);

            // 2. Idempotência: a segunda chamada não deve aplicar nada.
            await context.Database.MigrateAsync();
            await EstoqueMigrationsTestSupport.AssertHistoryAsync(
                context, EstoqueMigrationsTestSupport.MigrationsEsperadas);
            await EstoqueMigrationsTestSupport.AssertIntegrityAsync(context);
        }
        finally
        {
            EstoqueMigrationsTestSupport.DeleteDatabaseFiles(path);
        }
    }
}
