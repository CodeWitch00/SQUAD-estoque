using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class StartupEnvironmentTests
{
    [Theory]
    [InlineData("Development", true, 2)]
    [InlineData("Development", false, 0)]
    [InlineData("Production", true, 0)]
    [InlineData("Production", false, 0)]
    [InlineData("Testing", true, -1)]
    public async Task Inicializacao_respeita_ambiente_e_flag_de_seed(
        string ambiente, bool seedHabilitado, int usuariosEsperados)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"s4-be-003-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        var dbPath = Path.Combine(tempDir, "nested", "Estoque.db");

        try
        {
            using var factory = new StartupFactory(ambiente, dbPath, seedHabilitado);
            using var client = factory.CreateClient();
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);

            if (ambiente == "Testing")
            {
                Assert.False(File.Exists(dbPath));
            }
            else
            {
                using var scope = factory.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
                Assert.Equal(usuariosEsperados, await db.Usuario.CountAsync());
                Assert.Equal(db.Database.GetMigrations().Count(),
                    (await db.Database.GetAppliedMigrationsAsync()).Count());
            }
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Production_recusa_volume_indisponivel_antes_de_servir_requisicoes()
    {
        var root = Path.Combine(Path.GetTempPath(), $"storage-startup-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var blocked = Path.Combine(root, "bloqueado");
        File.WriteAllText(blocked, "preservar");
        try
        {
            using var factory = new StartupFactory("Production", Path.Combine(blocked, "Estoque.db"), false);
            var error = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());
            Assert.Contains("Armazenamento SQLite indisponivel", error.Message);
            Assert.Equal("preservar", File.ReadAllText(blocked));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public async Task Testing_mantem_usuarios_apenas_no_banco_isolado_da_factory()
    {
        using var factory = new SquadEstoqueWebApplicationFactory();
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EstoqueContext>();

        Assert.Equal(2, await db.Usuario.CountAsync());
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health")).StatusCode);
    }

    private sealed class StartupFactory(string ambiente, string dbPath, bool seedHabilitado)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(ambiente);
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AllowedHosts"] = "localhost",
                    ["ConnectionStrings:EstoqueContext"] = $"Data Source={dbPath};Pooling=False",
                    ["Demo:SeedUsers"] = seedHabilitado.ToString(),
                    ["DataProtection:KeysPath"] = Path.Combine(Path.GetDirectoryName(dbPath)!, "keys")
                }));
        }
    }
}
