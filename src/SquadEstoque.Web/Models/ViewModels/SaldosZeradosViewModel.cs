namespace SquadEstoque.Web.Models;

public class SaldosZeradosViewModel
{
    public List<ProdutoSaldoZeradoViewModel> Produtos { get; set; } = new();

    /// <summary>
    /// Compõe a visão com dados já materializados, sem acessar a persistência.
    /// Agrupa por Id (não pelo nome) e inclui somente numerações com saldo zero.
    /// Nomes, marcas, cores e numerações seguem ordem textual ordinal;
    /// o Id desempata produtos, independentemente da cultura do servidor.
    /// </summary>
    public static SaldosZeradosViewModel Criar(
        IReadOnlyCollection<ProdutoConsultaResultadoViewModel> produtos)
    {
        ArgumentNullException.ThrowIfNull(produtos);

        return new SaldosZeradosViewModel
        {
            Produtos = produtos
                .GroupBy(produto => produto.Id)
                .Select(grupo =>
                {
                    var produto = grupo.First();
                    return new ProdutoSaldoZeradoViewModel
                    {
                        Id = produto.Id,
                        Nome = produto.Nome,
                        Marca = produto.Marca,
                        Cor = produto.Cor,
                        NumeracoesZeradas = grupo
                            .SelectMany(item => item.Skus)
                            .Where(sku => sku.SaldoAtual == 0)
                            .Select(sku => sku.Numeracao)
                            .Distinct(StringComparer.Ordinal)
                            .OrderBy(numeracao => numeracao, StringComparer.Ordinal)
                            .ToList()
                    };
                })
                .Where(produto => produto.NumeracoesZeradas.Count > 0)
                .OrderBy(produto => produto.Nome, StringComparer.Ordinal)
                .ThenBy(produto => produto.Marca, StringComparer.Ordinal)
                .ThenBy(produto => produto.Cor, StringComparer.Ordinal)
                .ThenBy(produto => produto.Id)
                .ToList()
        };
    }
}

public class ProdutoSaldoZeradoViewModel
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public List<string> NumeracoesZeradas { get; set; } = new();
}
