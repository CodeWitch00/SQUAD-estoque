using System.Collections.Generic;

namespace SquadEstoque.Web.Models;

public class ProdutosIndexViewModel
{
    public IReadOnlyList<Produto> Produtos { get; set; } = new List<Produto>();
    public IReadOnlyList<string> Marcas { get; set; } = new List<string>();
    public IReadOnlyList<string> Categorias { get; set; } = new List<string>();

    public string? Busca { get; set; }
    public string? Status { get; set; }
    public string? Marca { get; set; }
    public string? Categoria { get; set; }
    public int PaginaAtual { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalItens { get; set; }
    public int TamanhoPagina { get; set; }

    public bool PossuiFiltros =>
        !string.IsNullOrWhiteSpace(Busca) ||
        !string.IsNullOrWhiteSpace(Status) ||
        !string.IsNullOrWhiteSpace(Marca) ||
        !string.IsNullOrWhiteSpace(Categoria);
}
