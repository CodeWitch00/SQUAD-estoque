using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Configuration;
using SquadEstoque.Web.Models;

var bootstrapLojista = args.Length > 0 && args[0] == "--bootstrap-lojista";
if (bootstrapLojista && args.Length != 3)
{
    throw new InvalidOperationException("Uso: --bootstrap-lojista NOME EMAIL (senha pela entrada padrao).");
}

var builder = WebApplication.CreateBuilder(bootstrapLojista ? Array.Empty<string>() : args);
if (bootstrapLojista && !builder.Environment.IsProduction())
{
    throw new InvalidOperationException("O primeiro LOJISTA so pode ser criado em Production.");
}

builder.Services.AddDataProtection();
builder.Services.AddOptions<Microsoft.AspNetCore.DataProtection.KeyManagement.KeyManagementOptions>()
    .Configure<IConfiguration, ILoggerFactory>((options, configuration, loggerFactory) =>
    {
        var keysPath = configuration["DataProtection:KeysPath"];
        if (!string.IsNullOrWhiteSpace(keysPath))
        {
            options.XmlRepository = new Microsoft.AspNetCore.DataProtection.Repositories.FileSystemXmlRepository(
                new DirectoryInfo(keysPath), loggerFactory);
        }
    });

builder.Services.AddDbContext<EstoqueContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("EstoqueContext") ?? throw new InvalidOperationException("Connection string 'EstoqueContext' not found.")));

// Configuração de autenticação por Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.SecurePolicy = builder.Environment.IsProduction()
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
    });

// Add services to the container.
var mvcBuilder = builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();

if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

var app = builder.Build();

ProductionConfiguration.Validate(app.Configuration, app.Environment);

// O modo automatico do host aceita cabecalhos de qualquer origem.
if (app.Configuration.GetValue<bool>("ForwardedHeaders_Enabled"))
{
    throw new InvalidOperationException("Desative ASPNETCORE_FORWARDEDHEADERS_ENABLED e configure ReverseProxy__KnownProxies.");
}

var forwardedHeaders = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto,
    ForwardLimit = 1
};
forwardedHeaders.KnownProxies.Clear();
forwardedHeaders.KnownIPNetworks.Clear();
foreach (var proxy in app.Configuration.GetSection("ReverseProxy:KnownProxies").GetChildren())
{
    if (!IPAddress.TryParse(proxy.Value, out var address) ||
        address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any) ||
        address.Equals(IPAddress.Any.MapToIPv6()))
    {
        throw new InvalidOperationException("ReverseProxy__KnownProxies deve listar enderecos IP explicitos.");
    }

    forwardedHeaders.KnownProxies.Add(address);
}

if (!app.Environment.IsEnvironment("Testing"))
{
    SqliteStorage.Prepare(app.Configuration, app.Environment.ContentRootPath);
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    // Garante que um ambiente novo possua o schema antes de consultar ou popular dados.
    var estoqueContext = services.GetRequiredService<EstoqueContext>();
    estoqueContext.Database.Migrate();

    if (bootstrapLojista)
    {
        var nome = args[1].Trim();
        var email = args[2].Trim();
        if (string.IsNullOrWhiteSpace(nome) || nome.Length > 150 ||
            string.IsNullOrWhiteSpace(email) || email.Length > 255 ||
            !new EmailAddressAttribute().IsValid(email))
        {
            throw new InvalidOperationException("Nome ou email do primeiro LOJISTA invalido.");
        }

        if (!Console.IsInputRedirected)
        {
            throw new InvalidOperationException("Forneca a senha pela entrada padrao redirecionada.");
        }

        using var transaction = estoqueContext.Database.BeginTransaction();
        if (estoqueContext.Usuario.Any())
        {
            throw new InvalidOperationException("O banco ja possui usuarios; primeiro acesso indisponivel.");
        }

        var senha = Console.In.ReadLine();
        if (senha is null || senha.Length < 12 || Encoding.UTF8.GetByteCount(senha) > 72)
        {
            throw new InvalidOperationException("A senha deve ter ao menos 12 caracteres e no maximo 72 bytes UTF-8.");
        }

        estoqueContext.Usuario.Add(new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Email = email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha, 12),
            Perfil = PerfilUsuario.LOJISTA
        });
        estoqueContext.SaveChanges();
        transaction.Commit();
        Console.WriteLine("Primeiro LOJISTA criado.");
        return;
    }

    // Usuários demonstrativos só podem ser criados em Development.
    var seedDemoUsers = builder.Configuration.GetValue<bool>("Demo:SeedUsers");
    if (app.Environment.IsDevelopment() && seedDemoUsers && !estoqueContext.Usuario.Any())
    {
        estoqueContext.Usuario.AddRange(
            new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = "Vendedor Teste",
                Email = "vendedor@squad.com",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("123", 12),
                Perfil = PerfilUsuario.VENDEDOR
            },
            new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = "Lojista Teste",
                Email = "lojista@squad.com",
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("123", 12),
                Perfil = PerfilUsuario.LOJISTA
            }
        );
        estoqueContext.SaveChanges();
    }
}

if (forwardedHeaders.KnownProxies.Count > 0)
{
    app.UseForwardedHeaders(forwardedHeaders);
}

if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    app.UseExceptionHandler("/Home/Error");
    if (app.Environment.IsProduction())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
}

app.UseRouting();

// Arquivos enviados em tempo de execução, como fotos de produtos, não constam
// do manifesto de assets gerado durante o build.
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

public partial class Program;
