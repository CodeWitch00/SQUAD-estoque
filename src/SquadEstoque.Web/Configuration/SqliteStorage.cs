using Microsoft.Data.Sqlite;

namespace SquadEstoque.Web.Configuration;

public static class SqliteStorage
{
    private const UnixFileMode PrivateDirectory = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
    private const UnixFileMode PrivateFile = UnixFileMode.UserRead | UnixFileMode.UserWrite;

    public static void Prepare(IConfiguration configuration, string contentRoot)
    {
        var value = configuration.GetConnectionString("EstoqueContext");
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Configuracao obrigatoria ausente: ConnectionStrings__EstoqueContext.");

        SqliteConnectionStringBuilder sqlite;
        try { sqlite = new SqliteConnectionStringBuilder(value); }
        catch (ArgumentException)
        {
            throw new InvalidOperationException("Configuracao invalida: ConnectionStrings__EstoqueContext deve ser uma connection string SQLite valida.");
        }

        if (string.IsNullOrWhiteSpace(sqlite.DataSource) || sqlite.DataSource == ":memory:" ||
            sqlite.Mode is SqliteOpenMode.Memory or SqliteOpenMode.ReadOnly ||
            sqlite.DataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("ConnectionStrings__EstoqueContext deve apontar para um arquivo SQLite gravavel; URI e banco em memoria nao sao aceitos.");

        var path = Path.GetFullPath(sqlite.DataSource, contentRoot);
        EnsureDirectory(Path.GetDirectoryName(path)!);
        try
        {
            // Abre sem truncar: confirma o acesso ao arquivo existente e cria com modo restrito.
            using (var file = OpenPrivateFile(path, sqlite.Mode == SqliteOpenMode.ReadWrite ? FileMode.Open : FileMode.OpenOrCreate)) { }
            sqlite.DataSource = path;
            sqlite.Pooling = false;
            using var connection = new SqliteConnection(sqlite.ConnectionString);
            connection.Open();
            // Confirma que o SQLite consegue obter uma transacao de escrita, sem alterar dados.
            using var transaction = connection.BeginTransaction(deferred: false);
            transaction.Rollback();
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or SqliteException)
        {
            throw Unavailable(path);
        }

        // Usa o mesmo caminho absoluto validado, inclusive em Development.
        sqlite.Pooling = new SqliteConnectionStringBuilder(value).Pooling;
        configuration["ConnectionStrings:EstoqueContext"] = sqlite.ConnectionString;

        var keysPath = configuration["DataProtection:KeysPath"];
        if (!string.IsNullOrWhiteSpace(keysPath))
        {
            keysPath = Path.GetFullPath(keysPath, contentRoot);
            EnsureDirectory(keysPath);
            configuration["DataProtection:KeysPath"] = keysPath;
        }
    }

    private static void EnsureDirectory(string path)
    {
        try
        {
            if (OperatingSystem.IsWindows()) Directory.CreateDirectory(path);
            else Directory.CreateDirectory(path, PrivateDirectory);

            var probePath = Path.Combine(path, $".storage-check-{Guid.NewGuid():N}");
            using var probe = OpenPrivateFile(probePath, FileMode.CreateNew, FileOptions.DeleteOnClose);
            probe.WriteByte(42);
            probe.Flush(flushToDisk: true);
            probe.Position = 0;
            if (probe.ReadByte() != 42) throw new IOException();
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw Unavailable(path);
        }
    }

    private static FileStream OpenPrivateFile(string path, FileMode mode, FileOptions options = FileOptions.None)
    {
        var settings = new FileStreamOptions
        {
            Mode = mode, Access = FileAccess.ReadWrite, Share = FileShare.ReadWrite, Options = options
        };
        if (!OperatingSystem.IsWindows() && mode != FileMode.Open) settings.UnixCreateMode = PrivateFile;
        return new FileStream(path, settings);
    }

    private static InvalidOperationException Unavailable(string path) => new(
        $"Armazenamento SQLite indisponivel em '{path}' para o usuario '{Environment.UserName}'. " +
        "Verifique a montagem do volume, espaco livre e permissoes de leitura, escrita e acesso ao diretorio. " +
        "Conceda acesso somente ao usuario do processo; nao utilize chmod 777.");
}
