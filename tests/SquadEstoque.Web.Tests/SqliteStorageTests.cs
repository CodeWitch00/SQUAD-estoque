using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using SquadEstoque.Web.Configuration;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class SqliteStorageTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"sqlite-storage-{Guid.NewGuid():N}");

    private IConfiguration Configuration(string source, string? keys = null, string mode = "ReadWriteCreate") =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:EstoqueContext"] = new SqliteConnectionStringBuilder
            { DataSource = source, Mode = Enum.Parse<SqliteOpenMode>(mode), Pooling = false }.ConnectionString,
            ["DataProtection:KeysPath"] = keys
        }).Build();

    [Fact]
    public void Cria_diretorios_e_preserva_dados_em_nova_inicializacao()
    {
        var path = Path.Combine(root, "dados", "Estoque.db");
        var keys = Path.Combine(root, "keys");
        var configuration = Configuration(path, keys);
        SqliteStorage.Prepare(configuration, root);
        using (var connection = new SqliteConnection(configuration.GetConnectionString("EstoqueContext")))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE evidencia (valor TEXT); INSERT INTO evidencia VALUES ('persistido');";
            command.ExecuteNonQuery();
        }
        SqliteStorage.Prepare(configuration, root);
        using var reopened = new SqliteConnection(configuration.GetConnectionString("EstoqueContext"));
        reopened.Open();
        using var query = reopened.CreateCommand();
        query.CommandText = "SELECT valor FROM evidencia";
        Assert.Equal("persistido", query.ExecuteScalar());
        Assert.True(Directory.Exists(keys));
        Assert.Empty(Directory.GetFiles(root, ".storage-check-*", SearchOption.AllDirectories));
        if (!OperatingSystem.IsWindows())
        {
            Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite, File.GetUnixFileMode(path));
            Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute,
                File.GetUnixFileMode(Path.GetDirectoryName(path)!));
        }
    }

    [Fact]
    public void Resolve_caminho_relativo_pela_raiz_da_aplicacao()
    {
        var configuration = Configuration("dados/Estoque.db");
        SqliteStorage.Prepare(configuration, root);
        Assert.Equal(Path.Combine(root, "dados", "Estoque.db"),
            new SqliteConnectionStringBuilder(configuration.GetConnectionString("EstoqueContext")).DataSource);
    }

    [Fact]
    public void Diretorio_ocupado_por_arquivo_falha_com_diagnostico()
    {
        Directory.CreateDirectory(root);
        var blocker = Path.Combine(root, "bloqueado");
        File.WriteAllText(blocker, "preservar");
        var error = Assert.Throws<InvalidOperationException>(() =>
            SqliteStorage.Prepare(Configuration(Path.Combine(blocker, "Estoque.db")), root));
        Assert.Contains("Armazenamento SQLite indisponivel", error.Message);
        Assert.Contains(blocker, error.Message);
        Assert.Contains(Environment.UserName, error.Message);
        Assert.Equal("preservar", File.ReadAllText(blocker));
    }

    [Fact]
    public void Banco_existente_sem_escrita_falha_sem_mudar_permissoes()
    {
        var path = Path.Combine(root, "Estoque.db");
        SqliteStorage.Prepare(Configuration(path), root);
        if (OperatingSystem.IsWindows()) File.SetAttributes(path, FileAttributes.ReadOnly);
        else File.SetUnixFileMode(path, UnixFileMode.UserRead);
        try
        {
            Assert.Throws<InvalidOperationException>(() => SqliteStorage.Prepare(Configuration(path), root));
        }
        finally
        {
            if (OperatingSystem.IsWindows()) File.SetAttributes(path, FileAttributes.Normal);
            else File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    [Theory]
    [InlineData(":memory:", "ReadWriteCreate")]
    [InlineData("", "ReadWriteCreate")]
    [InlineData("Estoque.db", "ReadOnly")]
    [InlineData("Estoque.db", "Memory")]
    [InlineData("file:Estoque.db", "ReadWriteCreate")]
    public void Rejeita_banco_nao_persistente_ou_somente_leitura(string source, string mode)
    {
        Assert.Throws<InvalidOperationException>(() => SqliteStorage.Prepare(Configuration(source, mode: mode), root));
        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public void Chaves_indisponiveis_impedem_inicializacao()
    {
        Directory.CreateDirectory(root);
        var keys = Path.Combine(root, "keys");
        File.WriteAllText(keys, "bloqueado");
        Assert.Throws<InvalidOperationException>(() =>
            SqliteStorage.Prepare(Configuration(Path.Combine(root, "Estoque.db"), keys), root));
    }

    public void Dispose()
    {
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }
}
