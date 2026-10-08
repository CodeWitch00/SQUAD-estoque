using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Models;

namespace SquadEstoque.Web.Data;

// Compartilha a operação sem introduzir uma camada entre controllers e contexto.
public static class SaidaEstoque
{
    // O futuro atendimento informa somente o SKU e o usuário autenticado.
    public static Task<ResultadoSaida> RegistrarVendaRapidaAsync(
        this EstoqueContext context, Guid skuId, Guid usuarioId) =>
        context.RegistrarSaidaAsync(skuId, 1, usuarioId);

    public static async Task<ResultadoSaida> RegistrarSaidaAsync(
        this EstoqueContext context, Guid skuId, int quantidade, Guid usuarioId,
        string? motivo = null)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("Informe o usuário autenticado.", nameof(usuarioId));

        if (quantidade < 1)
            return new(null, "A quantidade de saída deve ser no mínimo 1.", "Quantidade");

        try
        {
            return await RegistrarSaidaTransacionalAsync(context, skuId, quantidade, usuarioId, motivo);
        }
        catch (Exception exception) when (
            exception is DbUpdateConcurrencyException ||
            exception.GetBaseException() is SqliteException { SqliteErrorCode: 5 or 6 })
        {
            return new(null, "O estoque está sendo atualizado por outra operação. Tente novamente.",
                Conflito: true);
        }
    }

    private static async Task<ResultadoSaida> RegistrarSaidaTransacionalAsync(
        EstoqueContext context, Guid skuId, int quantidade, Guid usuarioId, string? motivo)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        Sku? sku = null;
        Movimentacao? movimentacao = null;

        try
        {
            // Saldo e movimentação só são confirmados juntos após o commit.
            var linhasAlteradas = await context.Sku
                .Where(s => s.Id == skuId && s.SaldoAtual >= quantidade)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.SaldoAtual, s => s.SaldoAtual - quantidade));
            sku = await context.Sku.Include(s => s.Produto)
                .FirstOrDefaultAsync(s => s.Id == skuId);

            if (sku == null || sku.Produto == null)
                return new(null, "O item de estoque selecionado não foi encontrado.");

            // ExecuteUpdate não sincroniza entidades já rastreadas pelo EF.
            await context.Entry(sku).ReloadAsync();
            if (linhasAlteradas == 0)
                return new(sku, $"Saldo insuficiente para saída. Saldo disponível: {sku.SaldoAtual} par(es).", "Quantidade");

            movimentacao = new Movimentacao
            {
                Id = Guid.NewGuid(),
                SkuId = sku.Id,
                Tipo = TipoMovimentacao.SAIDA,
                Quantidade = quantidade,
                UsuarioId = usuarioId,
                CriadoEm = DateTime.UtcNow,
                Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim()
            };
            context.Movimentacao.Add(movimentacao);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new(sku);
        }
        catch
        {
            await transaction.RollbackAsync();
            if (movimentacao != null)
                context.Entry(movimentacao).State = EntityState.Detached;
            if (sku != null)
                await context.Entry(sku).ReloadAsync();
            throw;
        }
    }
}

public sealed record ResultadoSaida(
    Sku? Sku, string? Erro = null, string Campo = "", bool Conflito = false);
