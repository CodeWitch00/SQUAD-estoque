using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class PrimeiroLojistaBootstrapTests
{
    [Fact]
    public async Task Production_cria_primeiro_lojista_sem_registrar_senha_e_recusa_segundo_bootstrap()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"s4-be-003-bootstrap-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var dbPath = Path.Combine(tempDir, "Estoque.db");
        const string senha = "senha-ficticia-unica-2026";

        try
        {
            var fraca = await RunBootstrap("Production", dbPath, "fraca-abc");
            Assert.NotEqual(0, fraca.ExitCode);
            Assert.DoesNotContain("fraca-abc", fraca.Output);

            var resultado = await RunBootstrap("Production", dbPath, senha);
            Assert.Equal(0, resultado.ExitCode);
            Assert.DoesNotContain(senha, resultado.Output);
            Assert.Contains("Primeiro LOJISTA criado.", resultado.Output);

            await using var connection = new SqliteConnection($"Data Source={dbPath}");
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT Email, SenhaHash, Perfil FROM Usuario";
            await using var reader = await command.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());
            Assert.Equal("primeiro@example.test", reader.GetString(0));
            var senhaHash = reader.GetString(1);
            Assert.True(BCrypt.Net.BCrypt.Verify(senha, senhaHash));
            Assert.DoesNotContain(senhaHash, resultado.Output);
            Assert.Equal(1, reader.GetInt32(2)); // LOJISTA
            Assert.False(await reader.ReadAsync());

            var repetido = await RunBootstrap("Production", dbPath, null);
            Assert.NotEqual(0, repetido.ExitCode);
            Assert.DoesNotContain(senha, repetido.Output);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task Comando_de_primeiro_acesso_recusa_Development()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"s4-be-003-bootstrap-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var dbPath = Path.Combine(tempDir, "Estoque.db");

        try
        {
            var resultado = await RunBootstrap("Development", dbPath, null);
            Assert.NotEqual(0, resultado.ExitCode);
            Assert.False(File.Exists(dbPath));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static async Task<(int ExitCode, string Output)> RunBootstrap(
        string ambiente, string dbPath, string? senha)
    {
        var assemblyPath = typeof(Program).Assembly.Location;
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = Path.GetDirectoryName(assemblyPath)!,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        start.ArgumentList.Add(assemblyPath);
        start.ArgumentList.Add("--bootstrap-lojista");
        start.ArgumentList.Add("Primeiro Lojista");
        start.ArgumentList.Add("primeiro@example.test");
        start.Environment["ASPNETCORE_ENVIRONMENT"] = ambiente;
        start.Environment["ConnectionStrings__EstoqueContext"] = $"Data Source={dbPath}";
        start.Environment["DataProtection__KeysPath"] = Path.Combine(Path.GetDirectoryName(dbPath)!, "keys");
        start.Environment["Demo__SeedUsers"] = "true";

        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        if (senha is not null)
        {
            await process.StandardInput.WriteLineAsync(senha);
        }
        process.StandardInput.Close();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await process.WaitForExitAsync(timeout.Token);
        return (process.ExitCode, await stdout + await stderr);
    }
}
