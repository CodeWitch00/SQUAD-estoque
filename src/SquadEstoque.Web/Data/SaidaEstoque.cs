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

        await using var transaction = await context.Database.BeginTransactionAsync();
        var sku = await context.Sku.Include(s => s.Produto)
            .FirstOrDefaultAsync(s => s.Id == skuId);

        if (sku == null || sku.Produto == null)
            return new(null, "O item de estoque selecionado não foi encontrado.");

        // Um SKU já rastreado não pode fornecer um saldo antigo à transação.
        await context.Entry(sku).ReloadAsync();
        if (quantidade > sku.SaldoAtual)
            return new(sku, $"Saldo insuficiente para saída. Saldo disponível: {sku.SaldoAtual} par(es).", "Quantidade");

        var saldoAnterior = sku.SaldoAtual;
        var movimentacao = new Movimentacao
        {
            Id = Guid.NewGuid(),
            SkuId = sku.Id,
            Tipo = TipoMovimentacao.SAIDA,
            Quantidade = quantidade,
            UsuarioId = usuarioId,
            CriadoEm = DateTime.UtcNow,
            Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim()
        };

        try
        {
            sku.SaldoAtual -= quantidade;
            context.Movimentacao.Add(movimentacao);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new(sku);
        }
        catch
        {
            await transaction.RollbackAsync();
            context.Entry(movimentacao).State = EntityState.Detached;
            sku.SaldoAtual = saldoAnterior;
            context.Entry(sku).Property(s => s.SaldoAtual).OriginalValue = saldoAnterior;
            throw;
        }
    }
}

public sealed record ResultadoSaida(Sku? Sku, string? Erro = null, string Campo = "");
