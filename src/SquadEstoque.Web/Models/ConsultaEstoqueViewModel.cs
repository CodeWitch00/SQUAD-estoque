using System;
using System.Collections.Generic;

namespace SquadEstoque.Web.Models;

public class ConsultaEstoqueViewModel
{
    public string Termo { get; set; } = string.Empty;
    public List<ProdutoConsultaResultadoViewModel> Resultados { get; set; } = new();
    public string? MensagemEstado { get; set; }
    public Guid? ProdutoSelecionadoId { get; set; }
}

public class ProdutoConsultaResultadoViewModel
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public List<SkuConsultaResultadoViewModel> Skus { get; set; } = new();
}

public class SkuConsultaResultadoViewModel
{
    public Guid Id { get; set; }
    public string Numeracao { get; set; } = string.Empty;
    public int SaldoAtual { get; set; }

    public string EstadoDisponibilidade => SaldoAtual switch
    {
        > 1 => "Disponível",
        1 => "Último par",
        _ => "Indisponível"
    };

    public string ClasseDisponibilidade => SaldoAtual switch
    {
        > 1 => "disponivel",
        1 => "ultimo-par",
        _ => "indisponivel"
    };
}
