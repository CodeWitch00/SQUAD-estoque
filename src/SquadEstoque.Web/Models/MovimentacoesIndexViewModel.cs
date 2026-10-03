using System;
using System.Collections.Generic;

namespace SquadEstoque.Web.Models;

public class MovimentacoesIndexViewModel
{
    public IReadOnlyList<Movimentacao> Movimentacoes { get; set; } = new List<Movimentacao>();
    public IReadOnlyList<MovimentacaoResponsavelOption> Responsaveis { get; set; } =
        new List<MovimentacaoResponsavelOption>();

    public string? Busca { get; set; }
    public string? Tipo { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public Guid? ResponsavelId { get; set; }
    public int PaginaAtual { get; set; }
    public int TotalPaginas { get; set; }
    public int TotalItens { get; set; }
    public int TamanhoPagina { get; set; }

    public bool PossuiFiltros =>
        !string.IsNullOrWhiteSpace(Busca) ||
        !string.IsNullOrWhiteSpace(Tipo) ||
        DataInicio.HasValue ||
        DataFim.HasValue ||
        ResponsavelId.HasValue;
}

public class MovimentacaoResponsavelOption
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}
