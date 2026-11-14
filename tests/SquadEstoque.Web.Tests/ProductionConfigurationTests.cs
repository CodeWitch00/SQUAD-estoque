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
        => await AssertRejected(connection, keys, "127.0.0.1;estoque.example.test", expected);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("*")]
    [InlineData("localhost;*")]
    [InlineData("*.example.test")]
    [InlineData("0.0.0.0")]
    [InlineData("[::]")]
    [InlineData("[0:0:0:0:0:0:0:0]")]
    [InlineData("localhost;")]
    [InlineData("localhost; estoque.example.test")]
    [InlineData("https://estoque.example.test")]
    [InlineData("estoque.example.test:8080")]
    [InlineData("estoque.example.test/path")]
    public async Task Production_recusa_AllowedHosts_ausente_ou_permissivo(string? allowedHosts)
        => await AssertRejected("valid", "keys", allowedHosts, "AllowedHosts");

    private static async Task AssertRejected(string? connection, string? keys, string? allowedHosts, string expected)
    {
        var directory = Path.Combine(Path.GetTempPath(), $"production-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            using var process = Start(directory, connection, keys, allowedHosts);
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            finally
            {
                if (!process.HasExited) process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }
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
                using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
                var response = await client.GetAsync(address + "/health", timeout.Token);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(timeout.Token));

                using var allowedRequest = new HttpRequestMessage(HttpMethod.Get, address + "/health");
                allowedRequest.Headers.Host = "estoque.example.test";
                using var allowedResponse = await client.SendAsync(allowedRequest, timeout.Token);
                Assert.Equal(HttpStatusCode.OK, allowedResponse.StatusCode);

                using var rejectedRequest = new HttpRequestMessage(HttpMethod.Get, address + "/health");
                rejectedRequest.Headers.Host = "fora-da-lista.example.test";
                using var rejectedResponse = await client.SendAsync(rejectedRequest, timeout.Token);
                Assert.Equal(HttpStatusCode.BadRequest, rejectedResponse.StatusCode);
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

    private static Process Start(string directory, string? connection, string? keys,
        string? allowedHosts = "127.0.0.1;estoque.example.test")
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
        if (allowedHosts is null) start.Environment.Remove("AllowedHosts");
        else start.Environment["AllowedHosts"] = allowedHosts;
        start.Environment["ConnectionStrings__EstoqueContext"] = connection == "valid"
            ? $"Data Source={Path.Combine(directory, "Estoque.db")}" : connection ?? "";
        start.Environment["DataProtection__KeysPath"] = keys == "keys"
            ? Path.Combine(directory, "keys") : keys ?? "";
        return Process.Start(start)!;
    }
}
