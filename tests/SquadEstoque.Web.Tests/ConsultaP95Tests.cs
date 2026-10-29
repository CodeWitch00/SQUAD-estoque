using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class ConsultaP95Tests
{
    public static double Percentile95(IEnumerable<double> samples)
    {
        var ordered = samples.Order().ToArray();
        if (ordered.Length == 0 || ordered.Any(v => !double.IsFinite(v) || v < 0))
            throw new ArgumentException("Amostras devem ser finitas, não negativas e não vazias.");
        return ordered[(int)Math.Ceiling(0.95 * ordered.Length) - 1];
    }

    [Fact]
    public void P95_uses_nearest_rank_and_keeps_slow_samples()
    {
        Assert.Equal(95, Percentile95(Enumerable.Range(1, 100).Reverse().Select(i => (double)i)));
        Assert.Equal(900, Percentile95(Enumerable.Repeat(1d, 94).Concat(Enumerable.Repeat(900d, 6))));
        Assert.Equal(500, Percentile95(new[] { 500d }));
        Assert.Throws<ArgumentException>(() => Percentile95(Array.Empty<double>()));
        Assert.Throws<ArgumentException>(() => Percentile95(new[] { double.NaN }));
    }

    [PerformanceFact]
    [Trait("Category", "Performance")]
    public async Task RNF02_consulta_p95_must_be_below_500ms()
    {
        var output = Path.GetFullPath(Environment.GetEnvironmentVariable("RNF02_OUTPUT")
            ?? Path.Combine("TestResults", "rnf02"));
        Directory.CreateDirectory(output);
        var samples = new List<Sample>();
        var warmups = new List<Sample>();
        var started = DateTimeOffset.UtcNow;
        for (var round = 1; round <= 3; round++)
        {
            using var factory = new PerformanceFactory();
            using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false, HandleCookies = true
            });
            client.Timeout = TimeSpan.FromSeconds(30);
            using var login = await client.GetAsync("/Account/Login");
            var html = await login.Content.ReadAsStringAsync();
            var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
            Assert.True(token.Success, "Token de login ausente.");
            using var body = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = "perf@example.test", ["Senha"] = "perf-local",
                ["__RequestVerificationToken"] = WebUtility.HtmlDecode(token.Groups[1].Value)
            });
            using var authenticated = await client.PostAsync("/Account/Login", body);
            Assert.Equal(HttpStatusCode.Found, authenticated.StatusCode);

            // Fixed interleaving avoids measuring every empty search after all positive searches.
            var scenarios = new[] { ("um", "unico", 1), ("dez", "dezgrupo", 10),
                ("cinquenta", "cinquentagrupo", 50), ("vazio", "inexistente", 0),
                ("grade", "unico&produtoId=" + ProductId(1), 1) };
            for (var repetition = 1; repetition <= 120; repetition++)
            foreach (var (name, query, expected) in scenarios)
            {
                var id = Guid.NewGuid().ToString("N");
                using var request = new HttpRequestMessage(HttpMethod.Get, "/Estoque/Consulta?termo=" + query);
                request.Headers.Add("X-Perf-Id", id);
                var watch = Stopwatch.StartNew();
                var status = 0;
                var valid = false;
                string? error = null;
                try
                {
                    using var response = await client.SendAsync(request);
                    var rawContent = await response.Content.ReadAsStringAsync();
                    watch.Stop();
                    var content = WebUtility.HtmlDecode(rawContent);
                    status = (int)response.StatusCode;
                    var count = name == "grade" ? Regex.Matches(content, "id=\"titulo-grade\"").Count
                        : Regex.Matches(content, "class=\"consulta-resultado ").Count;
                    valid = status == 200 && count == expected &&
                        (expected != 0 || content.Contains("Produto não encontrado.")) &&
                        (name != "grade" || Regex.Matches(content, "data-sku-id=\"").Count == 3);
                    if (!valid) error = $"HTTP {status}; produtos {count}; esperado {expected}";
                }
                catch (Exception ex) { error = ex.GetType().Name; }
                watch.Stop();
                var server = factory.Probe.Times.TryRemove(id, out var elapsed) ? elapsed : (double?)null;
                var sample = new Sample(round, name, repetition <= 20 ? repetition : repetition - 20,
                    server, watch.Elapsed.TotalMilliseconds, status, valid && server.HasValue, error);
                (repetition <= 20 ? warmups : samples).Add(sample);
            }
        }

        var summaries = samples.GroupBy(s => new { s.Round, s.Scenario }).Select(g => new
        {
            g.Key.Round, g.Key.Scenario, Count = g.Count(), Errors = g.Count(s => !s.Valid),
            ServerP95Ms = Percentile95(g.Select(s => s.ServerMs ?? s.ClientMs)),
            ClientP95Ms = Percentile95(g.Select(s => s.ClientMs)),
            MaximumServerMs = g.Max(s => s.ServerMs ?? s.ClientMs),
            Passed = g.All(s => s.Valid) && Percentile95(g.Select(s => s.ServerMs ?? s.ClientMs)) < 500
        }).ToArray();
        var passed = summaries.All(s => s.Passed) && warmups.All(s => s.Valid);
        var report = new
        {
            Requirement = "RNF-02", StartedUtc = started, FinishedUtc = DateTimeOffset.UtcNow,
            BaseCommit = Environment.GetEnvironmentVariable("RNF02_COMMIT") ?? "não informado",
            OS = RuntimeInformation.OSDescription, Runtime = RuntimeInformation.FrameworkDescription,
            Architecture = RuntimeInformation.ProcessArchitecture.ToString(), LogicalProcessors = Environment.ProcessorCount,
            Cpu = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"),
            AvailableMemoryBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
            Environment = "Testing; Release; TestServer in-process; SQLite em arquivo exclusivo por rodada; sem rede e sem navegador",
            Products = 10000, Skus = 30000, Users = 1, Rounds = 3,
            WarmupPerScenario = 20, MeasuredPerScenario = 100,
            Percentile = "nearest-rank: sort ascending, index ceil(0.95*N)-1; sem remoção de outliers",
            MissingServerTime = "mantém duração do cliente como substituta e reprova a amostra",
            Passed = passed, Summaries = summaries, Warmups = warmups, Samples = samples
        };
        await File.WriteAllTextAsync(Path.Combine(output, "resultado.json"),
            JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        var csv = new List<string> { "fase,rodada,cenario,repeticao,servidor_ms,cliente_ms,http,valido,erro" };
        foreach (var (phase, rows) in new[] { ("aquecimento", warmups), ("medicao", samples) })
        foreach (var s in rows)
            csv.Add(FormattableString.Invariant($"{phase},{s.Round},{s.Scenario},{s.Repetition},{s.ServerMs:F6},{s.ClientMs:F6},{s.Status},{s.Valid},{s.Error}"));
        await File.WriteAllLinesAsync(Path.Combine(output, "amostras.csv"), csv);
        Assert.True(passed, $"RNF-02 reprovado. Consulte {output}; nenhuma amostra foi removida.");
    }

    private static Guid ProductId(int i) => Guid.Parse($"00000000-0000-0000-0000-{i:000000000000}");
    private sealed record Sample(int Round, string Scenario, int Repetition, double? ServerMs,
        double ClientMs, int Status, bool Valid, string? Error);

    private sealed class PerformanceFactory : WebApplicationFactory<Program>
    {
        private readonly string database = Path.Combine(Path.GetTempPath(), $"rnf02-{Guid.NewGuid():N}.db");
        public TimingProbe Probe { get; } = new();
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureServices(services =>
            {
                services.AddDataProtection().UseEphemeralDataProtectionProvider();
                services.RemoveAll<DbContextOptions<EstoqueContext>>();
                services.RemoveAll<EstoqueContext>();
                services.AddDbContext<EstoqueContext>(o => o.UseSqlite($"Data Source={database};Pooling=False"));
                services.AddSingleton<IStartupFilter>(Probe);
            });
        }
        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);
            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EstoqueContext>();
            db.Database.EnsureCreated();
            db.Usuario.Add(new Usuario { Id = ProductId(20000), Nome = "Performance",
                Email = "perf@example.test", SenhaHash = BCrypt.Net.BCrypt.HashPassword("perf-local", 12),
                Perfil = PerfilUsuario.VENDEDOR });
            for (var i = 1; i <= 10000; i++)
            {
                var group = i == 1 ? "unico" : i <= 11 ? "dezgrupo" : i <= 61 ? "cinquentagrupo" : "catalogo";
                var product = new Produto { Id = ProductId(i), Nome = $"{group} modelo {i:00000}",
                    Marca = "Marca", Categoria = "Calcado", Cor = "Preto", Ativo = true };
                for (var size = 37; size <= 39; size++)
                    product.Skus.Add(new Sku { Id = ProductId(i * 3 + size + 30000), ProdutoId = product.Id,
                        Numeracao = size.ToString(CultureInfo.InvariantCulture), SaldoAtual = size - 37,
                        Ativo = true, UltimaAtualizacaoSaldoEm = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc) });
                db.Produto.Add(product);
            }
            db.SaveChanges();
            return host;
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                foreach (var suffix in new[] { "", "-wal", "-shm" }) File.Delete(database + suffix);
        }
    }

    private sealed class TimingProbe : IStartupFilter
    {
        public ConcurrentDictionary<string, double> Times { get; } = new();
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use(async (context, pipeline) =>
            {
                var id = context.Request.Headers["X-Perf-Id"].ToString();
                var watch = Stopwatch.StartNew();
                try { await pipeline(); }
                finally { if (id.Length > 0) Times[id] = watch.Elapsed.TotalMilliseconds; }
            });
            next(app);
        };
    }
}

public sealed class PerformanceFactAttribute : FactAttribute
{
    public PerformanceFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("RNF02_PERF") != "1")
            Skip = "Medição isolada: definir RNF02_PERF=1 e executar em Release; ver roteiro S3-QA-028.";
    }
}
