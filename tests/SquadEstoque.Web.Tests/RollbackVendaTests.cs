using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;
using Xunit;

namespace SquadEstoque.Web.Tests;

public sealed class RollbackVendaTests
{
    [Fact]
    public async Task Falha_controlada_antes_do_commit_preserva_saldo_e_nao_cria_movimentacao()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"s3-qa-004-{Guid.NewGuid():N}.db");
        var interceptor = new FalhaControladaAntesDoCommitInterceptor();
        var options = new DbContextOptionsBuilder<EstoqueContext>()
            .UseSqlite($"Data Source={databasePath};Pooling=False")
            .AddInterceptors(interceptor)
            .Options;

        try
        {
            Guid skuId;
            Guid usuarioId;
            await using (var setup = new EstoqueContext(options))
            {
                await setup.Database.EnsureCreatedAsync();
                var produto = new Produto
                {
                    Id = Guid.NewGuid(),
                    Nome = "Tênis rollback",
                    Marca = "Squad",
                    Categoria = "Calçado",
                    Cor = "Preto"
                };
                var sku = new Sku
                {
                    Id = Guid.NewGuid(),
                    ProdutoId = produto.Id,
                    Numeracao = "38",
                    SaldoAtual = 5
                };
                var usuario = new Usuario
                {
                    Id = Guid.NewGuid(),
                    Nome = "Vendedor rollback",
                    Email = $"{Guid.NewGuid():N}@example.test",
                    SenhaHash = "hash-teste",
                    Perfil = PerfilUsuario.VENDEDOR
                };
                setup.AddRange(produto, sku, usuario);
                await setup.SaveChangesAsync();
                skuId = sku.Id;
                usuarioId = usuario.Id;
            }

            interceptor.Ativo = true;
            await using (var venda = new EstoqueContext(options))
            {
                await Assert.ThrowsAsync<FalhaControladaException>(() =>
                    venda.RegistrarVendaRapidaAsync(skuId, usuarioId));

                Assert.Equal(1, interceptor.TentativasDeCommit);
                Assert.Equal(4, interceptor.SaldoDentroDaTransacao);
                Assert.Equal(1, interceptor.MovimentacoesDentroDaTransacao);
                Assert.Null(venda.Database.CurrentTransaction);
            }

            interceptor.Ativo = false;
            await using var verificacao = new EstoqueContext(options);
            Assert.Equal(5, await verificacao.Sku
                .Where(sku => sku.Id == skuId)
                .Select(sku => sku.SaldoAtual)
                .SingleAsync());
            Assert.Empty(await verificacao.Movimentacao.AsNoTracking().ToListAsync());
        }
        finally
        {
            File.Delete(databasePath);
            File.Delete(databasePath + "-wal");
            File.Delete(databasePath + "-shm");
        }
    }

    // Simula a falha de forma determinística no limite da transação: SaveChanges já
    // executou o UPDATE e o INSERT, mas o COMMIT ainda não foi enviado ao SQLite.
    private sealed class FalhaControladaAntesDoCommitInterceptor : DbTransactionInterceptor
    {
        public bool Ativo { get; set; }
        public int TentativasDeCommit { get; private set; }
        public int SaldoDentroDaTransacao { get; private set; }
        public int MovimentacoesDentroDaTransacao { get; private set; }

        public override async ValueTask<InterceptionResult> TransactionCommittingAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            if (!Ativo)
                return result;

            TentativasDeCommit++;
            await using var command = transaction.Connection!.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "SELECT SaldoAtual FROM Sku";
            SaldoDentroDaTransacao = Convert.ToInt32(
                await command.ExecuteScalarAsync(cancellationToken));
            command.CommandText = "SELECT COUNT(*) FROM Movimentacao";
            MovimentacoesDentroDaTransacao = Convert.ToInt32(
                await command.ExecuteScalarAsync(cancellationToken));

            throw new FalhaControladaException();
        }
    }

    private sealed class FalhaControladaException : Exception;
}
