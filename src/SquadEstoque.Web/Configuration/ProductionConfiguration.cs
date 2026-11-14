using System.Net;
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

        var allowedHosts = configuration["AllowedHosts"];
        if (string.IsNullOrWhiteSpace(allowedHosts))
            throw new InvalidOperationException("Configuracao obrigatoria ausente: AllowedHosts.");

        if (allowedHosts.Split(';').Any(IsInvalidHost))
            throw new InvalidOperationException("Configuracao invalida: AllowedHosts deve listar hosts explicitos separados por ponto e virgula, sem curingas, espacos, esquema, porta ou caminho.");
    }

    private static bool IsInvalidHost(string host)
    {
        var kind = Uri.CheckHostName(host);
        if (host.Any(char.IsWhiteSpace) || host.Contains('*') || kind == UriHostNameType.Unknown)
            return true;

        // O filtro padrao do ASP.NET Core trata estes enderecos como curingas globais.
        if (IPAddress.TryParse(host.Trim('[', ']'), out var address) &&
            (address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any)))
            return true;

        // O HostFiltering exige os colchetes em enderecos IPv6.
        return kind == UriHostNameType.IPv6 && !(host.StartsWith('[') && host.EndsWith(']'));
    }
}
