using System.Data.Common;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SquadEstoque.Web.Controllers;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;
using Xunit.Abstractions;

namespace SquadEstoque.Web.Tests;

public sealed class ConsultaQueryPerformanceTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(4)]
    [InlineData(200)]
    public async Task Resume_preserves_grade_with_constant_query_count(int skuCount)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var observer = new QueryObserver();
        var options = new DbContextOptionsBuilder<EstoqueContext>()
            .UseSqlite(connection).AddInterceptors(observer).Options;
        await using var context = new EstoqueContext(options);
        await context.Database.EnsureCreatedAsync();
        var user = new Usuario { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Nome = "Vendedor", Email = "perf@example.com", SenhaHash = "teste" };
        var product = new Produto { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Nome = "Tênis Runner", Marca = "Marca", Categoria = "Tênis", Cor = "Azul" };
        var updated = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
        product.Skus = Enumerable.Range(0, skuCount).Select(i => new Sku
        {
            Id = Guid.Parse($"33333333-3333-3333-3333-{i:D12}"), ProdutoId = product.Id, Numeracao = i.ToString("D3"),
            SaldoAtual = i % 3, Ativo = i % 2 == 0, UltimaAtualizacaoSaldoEm = updated
        }).ToList();
        var attendance = new Atendimento { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), UsuarioId = user.Id, ProdutoId = product.Id, SkuId = product.Skus.First().Id };
        context.AddRange(user, product, attendance);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var controller = new EstoqueController(context)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Role, "VENDEDOR") }, "test"))
            } }
        };

        async Task<ConsultaEstoqueViewModel> Execute()
        {
            observer.Commands.Clear();
            var view = Assert.IsType<ViewResult>(await controller.Consulta(null, null, attendance.Id));
            var model = Assert.IsType<ConsultaEstoqueViewModel>(view.Model);
            var result = Assert.Single(model.Resultados);
            Assert.Equal(attendance.Id, model.AtendimentoId);
            Assert.Equal(attendance.SkuId, model.SkuSelecionadoId);
            Assert.Equal(product.Id, model.ProdutoSelecionadoId);
            Assert.Equal(product.Nome, result.Nome);
            Assert.Equal(product.Skus.Where(s => s.Ativo).OrderBy(s => s.Numeracao).Select(s => (s.Id, s.Numeracao, s.SaldoAtual, s.UltimaAtualizacaoSaldoEm)),
                result.Skus.Select(s => (s.Id, s.Numeracao, s.SaldoAtual, s.UltimaAtualizacaoSaldoEm)));
            Assert.Equal(result.Skus.All(s => s.SaldoAtual == 0), model.ModeloTotalmenteIndisponivel);
            Assert.Single(model.AtendimentosPendentes);
            Assert.Empty(context.ChangeTracker.Entries());
            Assert.Equal(2, observer.Commands.Count);
            return model;
        }

        for (var i = 0; i < 5; i++) await Execute();
        var times = new List<double>();
        var allocations = new List<long>();
        for (var i = 0; i < 30; i++)
        {
            var allocated = GC.GetAllocatedBytesForCurrentThread();
            var watch = Stopwatch.StartNew();
            await Execute();
            watch.Stop();
            times.Add(watch.Elapsed.TotalMilliseconds);
            allocations.Add(GC.GetAllocatedBytesForCurrentThread() - allocated);
        }
        output.WriteLine($"SKUs={skuCount}; active={skuCount / 2}; queries={observer.Commands.Count}; median_ms={times.Order().ElementAt(15):F3}; median_bytes={allocations.Order().ElementAt(15)}; iterations=30; warmup=5");
        output.WriteLine(observer.Commands.Last());

        Assert.DoesNotContain("\"ConcluidoEm\"", observer.Commands.Last());
        Assert.DoesNotContain("\"SaldoConsultado\"", observer.Commands.Last());

        // The search and selected grade use a fixed number of queries, regardless of SKU count.
        observer.Commands.Clear();
        var search = Assert.IsType<ViewResult>(await controller.Consulta("tenis runner", product.Id, null));
        var searchModel = Assert.IsType<ConsultaEstoqueViewModel>(search.Model);
        Assert.Equal(product.Id, Assert.Single(searchModel.Resultados).Id);
        Assert.Equal(skuCount / 2, searchModel.Resultados[0].Skus.Count);
        Assert.Equal(3, observer.Commands.Count);
        Assert.Empty(context.ChangeTracker.Entries());

        await context.Atendimento.Where(a => a.Id == attendance.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Status, StatusAtendimento.CONCLUIDO)
                .SetProperty(a => a.Resultado, ResultadoAtendimento.DESISTIU)
                .SetProperty(a => a.ConcluidoEm, updated));
        var closed = Assert.IsType<ViewResult>(await controller.Consulta(null, null, attendance.Id));
        Assert.Equal("Atendimento pendente não encontrado.", Assert.IsType<ConsultaEstoqueViewModel>(closed.Model).MensagemEstado);
        await context.Atendimento.Where(a => a.Id == attendance.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.Status, StatusAtendimento.PENDENTE)
                .SetProperty(a => a.Resultado, (ResultadoAtendimento?)null)
                .SetProperty(a => a.ConcluidoEm, (DateTime?)null));

        // Ownership and status must still be checked before returning the grade.
        attendance.UsuarioId = Guid.NewGuid();
        // Use a valid second owner to keep the foreign key intact.
        context.Usuario.Add(new Usuario { Id = attendance.UsuarioId, Nome = "Outro", Email = "other@example.com", SenhaHash = "teste" });
        await context.SaveChangesAsync();
        await context.Atendimento.Where(a => a.Id == attendance.Id).ExecuteUpdateAsync(s => s.SetProperty(a => a.UsuarioId, attendance.UsuarioId));
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        var denied = Assert.IsType<ViewResult>(await controller.Consulta(null, null, attendance.Id));
        var deniedModel = Assert.IsType<ConsultaEstoqueViewModel>(denied.Model);
        Assert.Empty(deniedModel.Resultados);
        Assert.Equal("Atendimento pendente não encontrado.", deniedModel.MensagemEstado);
    }

    private sealed class QueryObserver : DbCommandInterceptor
    {
        public List<string> Commands { get; } = new();
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Commands.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }
    }
}
