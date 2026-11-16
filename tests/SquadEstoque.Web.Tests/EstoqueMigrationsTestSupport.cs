using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

// Apoio comum ao S4-DB-004.
//
// Regras desta validação:
// - usar a cadeia real de migrations (IMigrator/Migrate), nunca EnsureCreated;
// - usar SQLite em arquivo, não :memory:, porque o ambiente real é arquivo;
// - comparar colunas originais existentes na versão de partida (snapshot);
// - limpar somente arquivos temporários de teste, nunca o banco compartilhado.
internal static class EstoqueMigrationsTestSupport
{
    public const string Initial = "20260817120442_InitialSquadSchema";
    public const string EnumConstraints = "20260901000759_AddEnumDomainConstraints";
    public const string Atendimento = "20261009181737_AddAtendimentoAuditavel";
    public const string EntradaFiscal = "20261009200729_AddEntradaFiscalEImagemProduto";
    public const string UltimaAtualizacaoSaldo = "20261026220326_AddSkuUltimaAtualizacaoSaldo";
    public const string ConsultaIndexes = "20261029210004_AddConsultaIndexes";

    public static readonly string[] MigrationsEsperadas =
    {
        Initial, EnumConstraints, Atendimento, EntradaFiscal, UltimaAtualizacaoSaldo, ConsultaIndexes
    };

    private static readonly Guid UsuarioId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ProdutoId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Sku1Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid Sku2Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid MovimentacaoId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid RupturaId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    private static readonly Guid AtendimentoId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    private static readonly Guid EntradaFiscalId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    private static readonly Guid ItemEntradaFiscalId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    private static readonly Guid VinculoId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public static string CreateDatabasePath() =>
        Path.Combine(Path.GetTempPath(), $"estoque-migrations-{Guid.NewGuid():N}.db");

    public static DbContextOptions<EstoqueContext> CreateOptions(string path) =>
        new DbContextOptionsBuilder<EstoqueContext>()
            .UseSqlite(new SqliteConnectionStringBuilder
            {
                DataSource = path,
                Pooling = false,
                ForeignKeys = true
            }.ToString())
            .Options;

    public static async Task MigrateToAsync(EstoqueContext context, string? targetMigration)
    {
        var migrator = context.GetService<IMigrator>();
        if (targetMigration is null)
            await migrator.MigrateAsync();
        else
            await migrator.MigrateAsync(targetMigration);
    }

    public static async Task AssertHistoryAsync(EstoqueContext context, params string[] expected)
    {
        var applied = (await context.Database.GetAppliedMigrationsAsync()).OrderBy(id => id).ToArray();
        Assert.Equal(expected.OrderBy(id => id).ToArray(), applied);
    }

    public static async Task AssertIntegrityAsync(EstoqueContext context)
    {
        var integrity = await ScalarStringAsync(context,
            "SELECT integrity_check AS Value FROM pragma_integrity_check");
        Assert.Equal("ok", integrity);

        var fk = await ScalarLongAsync(context,
            "SELECT COUNT(*) AS Value FROM pragma_foreign_key_check");
        Assert.Equal(0, fk);
    }

    public static async Task AssertSchemaAsync(EstoqueContext context)
    {
        var tables = await StringListAsync(context,
            "SELECT name AS Value FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name");
        foreach (var expected in new[]
                 {
                     "Atendimento", "EntradaFiscal", "ItemEntradaFiscal", "Movimentacao",
                     "Produto", "Ruptura", "Sku", "Usuario", "VinculoProdutoFornecedor"
                 })
            Assert.Contains(expected, tables);

        var indexes = await StringListAsync(context,
            "SELECT name AS Value FROM sqlite_master WHERE type='index' AND name NOT LIKE 'sqlite_%' ORDER BY name");
        foreach (var expected in new[]
                 {
                     "IX_Produto_Marca", "IX_Produto_Categoria",
                     "IX_Sku_ProdutoId_SaldoZero", "IX_Ruptura_SkuId_CriadoEm"
                 })
            Assert.Contains(expected, indexes);

        var partial = await ScalarStringAsync(context,
            "SELECT sql AS Value FROM sqlite_master WHERE type='index' AND name='IX_Sku_ProdutoId_SaldoZero'");
        Assert.Contains("WHERE SaldoAtual = 0", partial);

        var skuSql = await ScalarStringAsync(context,
            "SELECT sql AS Value FROM sqlite_master WHERE type='table' AND name='Sku'");
        Assert.Contains("CHECK", skuSql);
        Assert.Contains("SaldoAtual >= 0", skuSql);
    }

    public static async Task AssertEscritaLeituraAsync(EstoqueContext context)
    {
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Usuário de Migration",
            Email = $"migration-{Guid.NewGuid():N}@example.test",
            SenhaHash = "hash-isolado-de-teste",
            Perfil = PerfilUsuario.LOJISTA
        };
        var produto = new Produto
        {
            Id = Guid.NewGuid(),
            Nome = "Produto de Migration",
            Marca = "Marca",
            Categoria = "Categoria",
            Cor = "Preto",
            Ativo = true
        };
        var sku = new Sku
        {
            Id = Guid.NewGuid(),
            ProdutoId = produto.Id,
            Numeracao = "38",
            SaldoAtual = 7,
            Ativo = true
        };
        var movimentacao = new Movimentacao
        {
            Id = Guid.NewGuid(),
            SkuId = sku.Id,
            Tipo = TipoMovimentacao.ENTRADA,
            Quantidade = 7,
            UsuarioId = usuario.Id,
            CriadoEm = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        context.AddRange(usuario, produto, sku, movimentacao);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        Assert.True(await context.Produto.AnyAsync(p => p.Id == produto.Id));
        Assert.Equal(7, await context.Sku.Where(s => s.Id == sku.Id).Select(s => s.SaldoAtual).SingleAsync());
        Assert.True(await context.Movimentacao.AnyAsync(m => m.Id == movimentacao.Id));
    }

    public static async Task AssertConstraintsAsync(EstoqueContext context)
    {
        // SKU duplicado (ProdutoId, Numeracao)
        await ExecAsync(context,
            "INSERT INTO Produto (Id, Nome, Marca, Categoria, Cor, Ativo) VALUES ('{0}','Pdup','M','C','Cor',1)",
            Guid.NewGuid());
        var pid = await ScalarStringAsync(context, "SELECT Id AS Value FROM Produto WHERE Nome='Pdup' LIMIT 1");
        await ExecAsync(context,
            "INSERT INTO Sku (Id, ProdutoId, Numeracao, SaldoAtual, Ativo, UltimaAtualizacaoSaldoEm) VALUES ('{0}','{1}','40',1,1,NULL)",
            Guid.NewGuid(), pid);
        await Assert.ThrowsAsync<DbUpdateException>(() => ExecAsync(context,
            "INSERT INTO Sku (Id, ProdutoId, Numeracao, SaldoAtual, Ativo, UltimaAtualizacaoSaldoEm) VALUES ('{0}','{1}','40',2,1,NULL)",
            Guid.NewGuid(), pid));

        // Saldo negativo
        await Assert.ThrowsAsync<DbUpdateException>(() => ExecAsync(context,
            "INSERT INTO Sku (Id, ProdutoId, Numeracao, SaldoAtual, Ativo, UltimaAtualizacaoSaldoEm) VALUES ('{0}','{1}','41',-1,1,NULL)",
            Guid.NewGuid(), pid));

        // Enum inválido (constraints adicionadas em AddEnumDomainConstraints)
        await Assert.ThrowsAsync<DbUpdateException>(() => ExecAsync(context,
            "INSERT INTO Usuario (Id, Nome, Email, SenhaHash, Perfil) VALUES ('{0}','U','perfil-invalido-{0}@x.test','h',99)",
            Guid.NewGuid()));
        var uid = await ScalarStringAsync(context, "SELECT Id AS Value FROM Usuario LIMIT 1");
        var sid = await ScalarStringAsync(context, "SELECT Id AS Value FROM Sku LIMIT 1");
        await Assert.ThrowsAsync<DbUpdateException>(() => ExecAsync(context,
            "INSERT INTO Movimentacao (Id, SkuId, Tipo, Quantidade, UsuarioId, CriadoEm, Motivo, AtendimentoId) VALUES ('{0}','{1}',99,1,'{2}','2026-01-01 00:00:00',NULL,NULL)",
            Guid.NewGuid(), sid, uid));
    }

    public static async Task SeedDadosLegadosAsync(EstoqueContext context, string versaoDePartida)
    {
        await ExecAsync(context,
            "INSERT INTO Usuario (Id, Nome, Email, SenhaHash, Perfil) VALUES ('{0}','Usuario Legado','legado@example.test','hash-legado',0)",
            UsuarioId);
        await ExecAsync(context,
            "INSERT INTO Produto (Id, Nome, Marca, Categoria, Cor, Ativo{1}) VALUES ('{0}','Produto Legado','Marca L','Cat L','Azul',1{2})",
            ProdutoId,
            HasColumn(versaoDePartida, "Produto", "ImagemUrl") ? ", ImagemUrl" : "",
            HasColumn(versaoDePartida, "Produto", "ImagemUrl") ? ", NULL" : "");
        await InsertSkuAsync(context, versaoDePartida, Sku1Id, ProdutoId, "39", 5);
        await InsertSkuAsync(context, versaoDePartida, Sku2Id, ProdutoId, "40", 3);

        var movCols = "Id, SkuId, Tipo, Quantidade, UsuarioId, CriadoEm, Motivo";
        var movVals = "'{0}','{1}',0,5,'{2}','2026-01-02 10:00:00','carga inicial'";
        if (HasColumn(versaoDePartida, "Movimentacao", "AtendimentoId")) { movCols += ", AtendimentoId"; movVals += ", NULL"; }
        await ExecAsync(context, $"INSERT INTO Movimentacao ({movCols}) VALUES ({movVals})", MovimentacaoId, Sku1Id, UsuarioId);

        var rupCols = "Id, SkuId, UsuarioId, CriadoEm";
        var rupVals = "'{0}','{1}','{2}','2026-01-03 10:00:00'";
        if (HasColumn(versaoDePartida, "Ruptura", "AtendimentoId")) { rupCols += ", AtendimentoId"; rupVals += ", NULL"; }
        await ExecAsync(context, $"INSERT INTO Ruptura ({rupCols}) VALUES ({rupVals})", RupturaId, Sku2Id, UsuarioId);

        if (TableExistsAt(versaoDePartida, "Atendimento"))
        {
            await ExecAsync(context,
                "INSERT INTO Atendimento (Id, UsuarioId, ProdutoId, SkuId, Status, Resultado, SaldoConsultado, CriadoEm, ConcluidoEm) " +
                "VALUES ('{0}','{1}','{2}','{3}',1,0,5,'2026-01-04 10:00:00','2026-01-04 10:05:00')",
                AtendimentoId, UsuarioId, ProdutoId, Sku1Id);
        }
        if (TableExistsAt(versaoDePartida, "EntradaFiscal"))
        {
            await ExecAsync(context,
                "INSERT INTO EntradaFiscal (Id, ChaveAcesso, Numero, CnpjFornecedor, FornecedorNome, Status, CriadoEm, ConfirmadoEm, UsuarioId) " +
                "VALUES ('{0}','12345678901234567890123456789012345678901234','NF1','12345678000199','Fornecedor',1,'2026-01-05 10:00:00',NULL,'{1}')",
                EntradaFiscalId, UsuarioId);
            await ExecAsync(context,
                "INSERT INTO ItemEntradaFiscal (Id, EntradaFiscalId, NumeroItem, CodigoFornecedor, Gtin, Descricao, Quantidade, ValorUnitario, ProdutoId, SkuId) " +
                "VALUES ('{0}','{1}',1,'CF1',NULL,'Item',2,10.0,'{2}','{3}')",
                ItemEntradaFiscalId, EntradaFiscalId, ProdutoId, Sku1Id);
            await ExecAsync(context,
                "INSERT INTO VinculoProdutoFornecedor (Id, CnpjFornecedor, CodigoFornecedor, SkuId) VALUES ('{0}','12345678000199','CF1','{1}')",
                VinculoId, Sku1Id);
        }
    }

    private static async Task InsertSkuAsync(EstoqueContext context, string versao, Guid id, Guid produtoId, string num, int saldo)
    {
        var cols = "Id, ProdutoId, Numeracao, SaldoAtual, Ativo";
        var vals = "'{0}','{1}','{2}',{3},1";
        if (HasColumn(versao, "Sku", "UltimaAtualizacaoSaldoEm")) { cols += ", UltimaAtualizacaoSaldoEm"; vals += ", NULL"; }
        await ExecAsync(context, $"INSERT INTO Sku ({cols}) VALUES ({vals})", id, produtoId, num, saldo);
    }

    public static async Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> CaptureSnapshotAsync(EstoqueContext context)
    {
        var snapshot = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        foreach (var table in new[] { "Usuario", "Produto", "Sku", "Movimentacao", "Ruptura", "Atendimento", "EntradaFiscal", "ItemEntradaFiscal", "VinculoProdutoFornecedor" })
        {
            if (!await TableExistsAsync(context, table)) continue;
            var cols = await StringListAsync(context,
                $"SELECT name AS Value FROM pragma_table_info('{table}') ORDER BY cid");
            var rows = await QueryRowsAsync(context,
                $"SELECT {string.Join(", ", cols.Select(c => QuoteIdent(c)))} FROM {QuoteIdent(table)} ORDER BY rowid");
            snapshot[table] = rows;
        }
        snapshot["__counts"] = new[]
        {
            $"SkuSum:{await ScalarLongAsync(context, "SELECT COALESCE(SUM(SaldoAtual),0) AS Value FROM Sku")}",
            $"SkuCount:{await ScalarLongAsync(context, "SELECT COUNT(*) AS Value FROM Sku")}",
            $"ProdutoCount:{await ScalarLongAsync(context, "SELECT COUNT(*) AS Value FROM Produto")}",
            $"MovCount:{await ScalarLongAsync(context, "SELECT COUNT(*) AS Value FROM Movimentacao")}",
            $"RupCount:{await ScalarLongAsync(context, "SELECT COUNT(*) AS Value FROM Ruptura")}"
        };
        return snapshot;
    }

    public static async Task AssertSnapshotPreservedAsync(
        EstoqueContext context, IReadOnlyDictionary<string, IReadOnlyList<string>> baseline)
    {
        var after = await CaptureSnapshotAsync(context);
        foreach (var kv in baseline)
        {
            Assert.True(after.ContainsKey(kv.Key), $"Tabela/chave ausente após migration: {kv.Key}");
            Assert.Equal(kv.Value, after[kv.Key]);
        }
    }

    public static async Task AssertFluxoEssencialAsync(EstoqueContext context)
    {
        // Exercita fluxo real de saída após atualização, sem alterar os registros legados semeados.
        var produto = new Produto { Id = Guid.NewGuid(), Nome = "Fluxo", Marca = "M", Categoria = "C", Cor = "Preta", Ativo = true };
        var sku = new Sku { Id = Guid.NewGuid(), ProdutoId = produto.Id, Numeracao = "42", SaldoAtual = 2, Ativo = true };
        var usuario = new Usuario { Id = Guid.NewGuid(), Nome = "Vendedor", Email = $"fluxo-{Guid.NewGuid():N}@x.test", SenhaHash = "h", Perfil = PerfilUsuario.VENDEDOR };
        context.AddRange(produto, sku, usuario);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var resultado = await context.RegistrarSaidaAsync(sku.Id, 1, usuario.Id, motivo: "teste-pos-migration");
        Assert.Null(resultado.Erro);
        Assert.NotNull(resultado.Sku);
        Assert.Equal(1, resultado.Sku!.SaldoAtual);

        context.ChangeTracker.Clear();
        Assert.Equal(1, await context.Sku.Where(s => s.Id == sku.Id).Select(s => s.SaldoAtual).SingleAsync());
        Assert.Equal(1, await context.Movimentacao.CountAsync(m => m.SkuId == sku.Id && m.Tipo == TipoMovimentacao.SAIDA && m.Quantidade == 1));
    }

    public static void DeleteDatabaseFiles(string path)
    {
        // Limpeza exclusiva de artefatos temporários de teste.
        foreach (var suffix in new[] { string.Empty, "-wal", "-shm" })
        {
            var file = path + suffix;
            if (File.Exists(file)) File.Delete(file);
        }
    }

    // --- helpers ---

    private static bool TableExistsAt(string versao, string table) => versao switch
    {
        Initial or EnumConstraints => table is "Usuario" or "Produto" or "Sku" or "Movimentacao" or "Ruptura",
        Atendimento => table is "Usuario" or "Produto" or "Sku" or "Movimentacao" or "Ruptura" or "Atendimento",
        EntradaFiscal or UltimaAtualizacaoSaldo or ConsultaIndexes => true,
        _ => table is "Usuario" or "Produto" or "Sku" or "Movimentacao" or "Ruptura"
    };

    private static bool HasColumn(string versao, string table, string column) => (table, column) switch
    {
        ("Produto", "ImagemUrl") => string.CompareOrdinal(versao, EntradaFiscal) >= 0,
        ("Sku", "UltimaAtualizacaoSaldoEm") => string.CompareOrdinal(versao, UltimaAtualizacaoSaldo) >= 0,
        ("Movimentacao", "AtendimentoId") => string.CompareOrdinal(versao, Atendimento) >= 0,
        ("Ruptura", "AtendimentoId") => string.CompareOrdinal(versao, Atendimento) >= 0,
        _ => true
    };

    private static async Task<bool> TableExistsAsync(EstoqueContext context, string table)
    {
        var n = await ScalarLongAsync(context,
            $"SELECT COUNT(*) AS Value FROM sqlite_master WHERE type='table' AND name='{table}'");
        return n > 0;
    }

    private static async Task ExecAsync(EstoqueContext context, string sqlTemplate, params object[] args)
    {
        var sql = args.Length == 0 ? sqlTemplate : string.Format(CultureInfo.InvariantCulture, sqlTemplate, args);
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static async Task<string> ScalarStringAsync(EstoqueContext context, string sql)
    {
        var conn = context.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var val = await cmd.ExecuteScalarAsync();
        return val is null or DBNull ? string.Empty : Convert.ToString(val, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static async Task<long> ScalarLongAsync(EstoqueContext context, string sql)
    {
        var conn = context.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        var val = await cmd.ExecuteScalarAsync();
        return val is null or DBNull ? 0 : Convert.ToInt64(val, CultureInfo.InvariantCulture);
    }

    private static async Task<List<string>> StringListAsync(EstoqueContext context, string sql)
    {
        var list = new List<string>();
        var conn = context.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(reader.IsDBNull(0) ? string.Empty : Convert.ToString(reader.GetValue(0), CultureInfo.InvariantCulture) ?? string.Empty);
        return list;
    }

    private static async Task<List<string>> QueryRowsAsync(EstoqueContext context, string sql)
    {
        var list = new List<string>();
        var conn = context.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var parts = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                parts[i] = reader.IsDBNull(i) ? "<NULL>" : (Convert.ToString(reader.GetValue(i), CultureInfo.InvariantCulture) ?? "");
            list.Add(string.Join("|", parts));
        }
        return list;
    }

    private static string QuoteIdent(string name) => "\"" + name.Replace("\"", "\"\"") + "\"";
}
