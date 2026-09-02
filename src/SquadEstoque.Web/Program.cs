using System;
using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;

var builder = WebApplication.CreateBuilder(args);

var dataProtectionKeysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    Directory.CreateDirectory(dataProtectionKeysPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
}

builder.Services.AddDbContext<LegacyMovieContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LegacyMovieContext") ?? throw new InvalidOperationException("Connection string 'LegacyMovieContext' not found.")));

builder.Services.AddDbContext<EstoqueContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("EstoqueContext") ?? throw new InvalidOperationException("Connection string 'EstoqueContext' not found.")));

// Configuração de autenticação por Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
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

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    // Garante que um ambiente novo possua o schema antes de consultar ou popular dados.
    var legacyMovieContext = services.GetRequiredService<LegacyMovieContext>();
    legacyMovieContext.Database.Migrate();
    SeedData.Initialize(services);

    var estoqueContext = services.GetRequiredService<EstoqueContext>();
    estoqueContext.Database.Migrate();

    // Usuários conhecidos só são criados em ambientes explicitamente demonstrativos.
    var seedDemoUsers = builder.Configuration.GetValue<bool>("Demo:SeedUsers");
    if (seedDemoUsers && !estoqueContext.Usuario.Any())
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

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
