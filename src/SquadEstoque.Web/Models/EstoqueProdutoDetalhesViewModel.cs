namespace SquadEstoque.Web.Models;

public sealed class EstoqueProdutoDetalhesViewModel
{
    public Guid ProdutoId { get; init; }
    public string Nome { get; init; } = string.Empty;
    public IReadOnlyList<EstoqueSkuDetalhesViewModel> Skus { get; init; } = [];
}

public sealed class EstoqueSkuDetalhesViewModel
{
    public string Numeracao { get; init; } = string.Empty;
    public int SaldoAtual { get; init; }
}
