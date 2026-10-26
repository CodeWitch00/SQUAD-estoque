using System;
using System.ComponentModel.DataAnnotations;

namespace SquadEstoque.Web.Models;

public class MovimentacaoCreateViewModel
{
    [Required(ErrorMessage = "Selecione o SKU.")]
    public Guid SkuId { get; set; }

    public string ProdutoNome { get; set; } = string.Empty;
    public string Numeracao { get; set; } = string.Empty;
    public int SaldoAtual { get; set; }

    public TipoMovimentacao Tipo { get; set; }

    [Required(ErrorMessage = "Informe a quantidade.")]
    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser no mínimo 1.")]
    public int Quantidade { get; set; } = 1;

    [StringLength(500, ErrorMessage = "O motivo/observação não pode exceder 500 caracteres.")]
    public string? Motivo { get; set; }
}
