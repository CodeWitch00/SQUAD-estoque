using Microsoft.Data.Sqlite;

namespace SquadEstoque.Web.Configuration;

public static class ProductionConfiguration
{
    public static void Validate(IConfiguration configuration, IHostEnvironment environment)
    {
        if (!environment.IsProduction()) return;

        var connectionString = configuration.GetConnectionString("EstoqueContext");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Configuracao obrigatoria ausente: ConnectionStrings__EstoqueContext.");

        // Nunca propagar a mensagem do parser: ela pode conter valores sensiveis.
        SqliteConnectionStringBuilder sqlite;
        try
        {
            sqlite = new SqliteConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException)
        {
            throw new InvalidOperationException("Configuracao invalida: ConnectionStrings__EstoqueContext deve ser uma connection string SQLite valida.");
        }

        if (!Path.IsPathFullyQualified(sqlite.DataSource) || sqlite.Mode == SqliteOpenMode.Memory)
            throw new InvalidOperationException("Configuracao invalida: ConnectionStrings__EstoqueContext deve apontar para um arquivo persistente com caminho absoluto.");

        var keysPath = configuration["DataProtection:KeysPath"];
        if (string.IsNullOrWhiteSpace(keysPath))
            throw new InvalidOperationException("Configuracao obrigatoria ausente: DataProtection__KeysPath.");
        if (!Path.IsPathFullyQualified(keysPath))
            throw new InvalidOperationException("Configuracao invalida: DataProtection__KeysPath deve ser um caminho absoluto persistente.");
    }
}
