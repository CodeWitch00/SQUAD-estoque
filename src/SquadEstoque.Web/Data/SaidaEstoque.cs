using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Models;

namespace SquadEstoque.Web.Data;

// Compartilha a operação sem introduzir uma camada entre controllers e contexto.
public static class SaidaEstoque
{
    // O futuro atendimento informa somente o SKU e o usuário autenticado.
    public static Task<ResultadoSaida> RegistrarVendaRapidaAsync(
        this EstoqueContext context, Guid skuId, Guid usuarioId, Guid? atendimentoId = null) =>
        context.RegistrarSaidaAsync(skuId, 1, usuarioId, atendimentoId: atendimentoId);

    public static async Task<ResultadoSaida> RegistrarSaidaAsync(
        this EstoqueContext context, Guid skuId, int quantidade, Guid usuarioId,
        string? motivo = null, Guid? atendimentoId = null)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("Informe o usuário autenticado.", nameof(usuarioId));

        if (quantidade < 1)
            return new(null, "A quantidade de saída deve ser no mínimo 1.", "Quantidade");

        try
        {
            return await RegistrarSaidaTransacionalAsync(
                context, skuId, quantidade, usuarioId, motivo, atendimentoId);
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
        EstoqueContext context, Guid skuId, int quantidade, Guid usuarioId,
        string? motivo, Guid? atendimentoId)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        Sku? sku = null;
        Movimentacao? movimentacao = null;
        Atendimento? atendimento = null;

        try
        {
            if (atendimentoId.HasValue)
            {
                atendimento = await context.Atendimento.FirstOrDefaultAsync(a =>
                    a.Id == atendimentoId.Value && a.UsuarioId == usuarioId &&
                    a.Status == StatusAtendimento.PENDENTE);
                if (atendimento is null)
                    return new(null, "O atendimento não está pendente ou não pertence ao vendedor.");
            }

            // Saldo e movimentação só são confirmados juntos após o commit.
            var linhasAlteradas = await context.Sku
                .Where(s => s.Id == skuId && s.SaldoAtual >= quantidade)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.SaldoAtual, s => s.SaldoAtual - quantidade));
            sku = await context.Sku.Include(s => s.Produto)
                .FirstOrDefaultAsync(s => s.Id == skuId);

            if (sku == null || sku.Produto == null)
                return new(null, "O item de estoque selecionado não foi encontrado.");

            if (atendimento is not null && atendimento.ProdutoId != sku.ProdutoId)
                return new(null, "O SKU selecionado não pertence ao atendimento.");

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
                Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim(),
                AtendimentoId = atendimentoId
            };
            if (atendimento is not null)
            {
                atendimento.SkuId = sku.Id;
                atendimento.SaldoConsultado = sku.SaldoAtual + quantidade;
                atendimento.Status = StatusAtendimento.CONCLUIDO;
                atendimento.Resultado = ResultadoAtendimento.VENDEU;
                atendimento.ConcluidoEm = DateTime.UtcNow;
            }
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
