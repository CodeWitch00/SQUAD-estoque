using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Data;
using Xunit;

namespace SquadEstoque.Web.Tests;

// S4-DB-004 — Cenário B: banco existente (atualização sem perda de dados).
public sealed class EstoqueMigrationsAtualizacaoTests
{
    public static IEnumerable<object[]> VersoesDePartida()
    {
        // A versão realmente implantada no ambiente compartilhado deve ser
        // confirmada em __EFMigrationsHistory antes da execução.
        yield return new object[] { EstoqueMigrationsTestSupport.Initial };
        yield return new object[] { EstoqueMigrationsTestSupport.EnumConstraints };
        yield return new object[] { EstoqueMigrationsTestSupport.Atendimento };
        yield return new object[] { EstoqueMigrationsTestSupport.EntradaFiscal };
        yield return new object[] { EstoqueMigrationsTestSupport.UltimaAtualizacaoSaldo };
    }

    [Theory]
    [MemberData(nameof(VersoesDePartida))]
    public async Task Banco_existente_na_versao_de_partida_atualiza_sem_perder_dados(string versaoDePartida)
    {
        var path = EstoqueMigrationsTestSupport.CreateDatabasePath();

        try
        {
            var options = EstoqueMigrationsTestSupport.CreateOptions(path);
            await using var context = new EstoqueContext(options);

            // 1. Banco "existente": para na versão de partida e recebe dados.
            await EstoqueMigrationsTestSupport.MigrateToAsync(context, versaoDePartida);
            await EstoqueMigrationsTestSupport.SeedDadosLegadosAsync(context, versaoDePartida);
            var baseline = await EstoqueMigrationsTestSupport.CaptureSnapshotAsync(context);

            // 2. Atualização até a última migration.
            await EstoqueMigrationsTestSupport.MigrateToAsync(context, targetMigration: null);

            // 3. Histórico completo.
            await EstoqueMigrationsTestSupport.AssertHistoryAsync(
                context, EstoqueMigrationsTestSupport.MigrationsEsperadas);

            // 4. Dados originais preservados (contagens/somas/amostras/colunas originais).
            await EstoqueMigrationsTestSupport.AssertSnapshotPreservedAsync(context, baseline);

            // 5. Schema, integridade e fluxo essencial após a atualização.
            await EstoqueMigrationsTestSupport.AssertSchemaAsync(context);
            await EstoqueMigrationsTestSupport.AssertIntegrityAsync(context);
            await EstoqueMigrationsTestSupport.AssertFluxoEssencialAsync(context);
        }
        finally
        {
            EstoqueMigrationsTestSupport.DeleteDatabaseFiles(path);
        }
    }

    [Fact]
    public async Task Dado_legado_invalido_falha_a_migration_sem_apagar_dados()
    {
        var path = EstoqueMigrationsTestSupport.CreateDatabasePath();

        try
        {
            var options = EstoqueMigrationsTestSupport.CreateOptions(path);
            await using (var context = new EstoqueContext(options))
            {
                // 1. Migrar até InitialSquadSchema (antes das constraints de enum).
                await EstoqueMigrationsTestSupport.MigrateToAsync(context, EstoqueMigrationsTestSupport.Initial);

                // 2. Inserir dado legado inválido (Perfil fora do domínio 0,1).
                await context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO Usuario (Id, Nome, Email, SenhaHash, Perfil) VALUES ('aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee','Invalido','invalido@example.test','h',2)");

                // 3. Tentar aplicar AddEnumDomainConstraints deve falhar.
                var ex = await Assert.ThrowsAnyAsync<Exception>(() =>
                    EstoqueMigrationsTestSupport.MigrateToAsync(context, EstoqueMigrationsTestSupport.EnumConstraints));
                Assert.True(ex is SqliteException || ex is DbUpdateException || ex.InnerException is SqliteException,
                    $"Esperava falha SQLite/EF, obtido: {ex}");
            }

            // 4-6. Reabrir e comprovar: migration não aplicada e dados preservados.
            await using (var verify = new EstoqueContext(options))
            {
                var applied = (await verify.Database.GetAppliedMigrationsAsync()).ToArray();
                Assert.Contains(EstoqueMigrationsTestSupport.Initial, applied);
                Assert.DoesNotContain(EstoqueMigrationsTestSupport.EnumConstraints, applied);

                var perfil = await verify.Database.SqlQueryRaw<int>(
                    "SELECT Perfil AS Value FROM Usuario WHERE Email='invalido@example.test'").SingleAsync();
                Assert.Equal(2, perfil);

                // 7. Registrar estado de artefatos temporários/lock (se existirem).
                var tempTables = await verify.Database.SqlQueryRaw<string>(
                    "SELECT name AS Value FROM sqlite_master WHERE name LIKE 'ef_temp_%'").ToListAsync();
                // Não deve haver tabelas temporárias residuais após falha tratada.
                Assert.Empty(tempTables);
            }
        }
        finally
        {
            EstoqueMigrationsTestSupport.DeleteDatabaseFiles(path);
        }
    }
}
