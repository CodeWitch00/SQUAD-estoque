using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;

namespace SquadEstoque.Web.Tests;

public sealed class SquadEstoqueWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _estoqueConnection = new("Data Source=:memory:");
    private readonly SqliteConnection _legacyMovieConnection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        _estoqueConnection.Open();
        _legacyMovieConnection.Open();

        builder.ConfigureServices(services =>
        {
            services.AddDataProtection()
                .UseEphemeralDataProtectionProvider();

            services.RemoveAll<DbContextOptions<EstoqueContext>>();
            services.RemoveAll<EstoqueContext>();
            services.RemoveAll<DbContextOptions<LegacyMovieContext>>();
            services.RemoveAll<LegacyMovieContext>();

            services.AddDbContext<EstoqueContext>(options =>
                options.UseSqlite(_estoqueConnection));
            services.AddDbContext<LegacyMovieContext>(options =>
                options.UseSqlite(_legacyMovieConnection));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var estoqueContext = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
        var legacyMovieContext = scope.ServiceProvider.GetRequiredService<LegacyMovieContext>();
        estoqueContext.Database.EnsureCreated();
        legacyMovieContext.Database.EnsureCreated();

        estoqueContext.Usuario.AddRange(
            CreateUser("Lojista Teste", "lojista@squad.com", PerfilUsuario.LOJISTA),
            CreateUser("Vendedor Teste", "vendedor@squad.com", PerfilUsuario.VENDEDOR));
        estoqueContext.Produto.Add(CreateProdutoConsulta());
        estoqueContext.SaveChanges();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _estoqueConnection.Dispose();
            _legacyMovieConnection.Dispose();
        }
    }

    private static Usuario CreateUser(string nome, string email, PerfilUsuario perfil)
    {
        return new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Email = email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("123", 12),
            Perfil = perfil
        };
    }

    private static Produto CreateProdutoConsulta()
    {
        var produto = new Produto
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Nome = "Tênis Runner",
            Marca = "Squad",
            Categoria = "Calçado",
            Cor = "Preto",
            Ativo = true
        };

        produto.Skus.Add(new Sku { Id = Guid.NewGuid(), ProdutoId = produto.Id, Numeracao = "37", SaldoAtual = 0 });
        produto.Skus.Add(new Sku { Id = Guid.NewGuid(), ProdutoId = produto.Id, Numeracao = "38", SaldoAtual = 1 });
        produto.Skus.Add(new Sku { Id = Guid.NewGuid(), ProdutoId = produto.Id, Numeracao = "39", SaldoAtual = 2 });

        return produto;
    }
}
