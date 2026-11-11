using System.Diagnostics;
using System.Net;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class ProductionConfigurationTests
{
    [Theory]
    [InlineData(null, "keys", "ConnectionStrings__EstoqueContext")]
    [InlineData(" ", "keys", "ConnectionStrings__EstoqueContext")]
    [InlineData("valid", null, "DataProtection__KeysPath")]
    [InlineData("valid", " ", "DataProtection__KeysPath")]
    [InlineData("Data Source=relative.db", "keys", "caminho absoluto")]
    [InlineData("Data Source=:memory:", "keys", "arquivo persistente")]
    [InlineData("invalid-secret=value", "keys", "SQLite valida")]
    [InlineData("valid", "relative", "DataProtection__KeysPath")]
    public async Task Production_recusa_configuracao_ausente_ou_invalida_sem_expor_valores(
        string? connection, string? keys, string expected)
    {
        var directory = Path.Combine(Path.GetTempPath(), $"production-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            using var process = Start(directory, connection, keys);
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await process.WaitForExitAsync(timeout.Token);
            var output = await stdout + await stderr;
            Assert.NotEqual(0, process.ExitCode);
            Assert.Contains(expected, output);
            Assert.DoesNotContain("invalid-secret=value", output);
            Assert.False(File.Exists(Path.Combine(directory, "Estoque.db")));
            Assert.False(Directory.Exists(Path.Combine(directory, "keys")));
        }
        finally { await DeleteDirectory(directory); }
    }

    [Fact]
    public async Task Production_inicia_por_variaveis_de_ambiente_e_responde_health()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"production-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            using var process = Start(directory, "valid", "keys");
            var listening = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            var stdout = Task.Run(async () =>
            {
                while (await process.StandardOutput.ReadLineAsync() is { } line)
                {
                    const string marker = "Now listening on: ";
                    var index = line.IndexOf(marker, StringComparison.Ordinal);
                    if (index >= 0) listening.TrySetResult(line[(index + marker.Length)..].Trim());
                }
                listening.TrySetException(new InvalidOperationException("Aplicacao encerrou antes de iniciar o servidor."));
            });
            var stderr = process.StandardError.ReadToEndAsync();
            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var address = await listening.Task.WaitAsync(timeout.Token);
                using var client = new HttpClient();
                var response = await client.GetAsync(address + "/health", timeout.Token);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(timeout.Token));
                Assert.True(File.Exists(Path.Combine(directory, "Estoque.db")));
                Assert.True(Directory.Exists(Path.Combine(directory, "keys")));
            }
            finally
            {
                if (!process.HasExited) process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
                await Task.WhenAll(stdout, stderr);
            }
        }
        finally { await DeleteDirectory(directory); }
    }

    private static async Task DeleteDirectory(string directory)
    {
        // O encerramento do processo pode liberar os arquivos WAL/SHM com atraso no Windows.
        for (var attempt = 0; ; attempt++)
        {
            try { Directory.Delete(directory, recursive: true); return; }
            catch (IOException) when (attempt < 10) { await Task.Delay(100); }
            catch (UnauthorizedAccessException) when (attempt < 10) { await Task.Delay(100); }
        }
    }

    private static Process Start(string directory, string? connection, string? keys)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        start.ArgumentList.Add(typeof(Program).Assembly.Location);
        start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        start.Environment["DOTNET_ENVIRONMENT"] = "Production";
        start.Environment["ASPNETCORE_URLS"] = "http://127.0.0.1:0";
        start.Environment["ConnectionStrings__EstoqueContext"] = connection == "valid"
            ? $"Data Source={Path.Combine(directory, "Estoque.db")}" : connection ?? "";
        start.Environment["DataProtection__KeysPath"] = keys == "keys"
            ? Path.Combine(directory, "keys") : keys ?? "";
        return Process.Start(start)!;
    }
}
