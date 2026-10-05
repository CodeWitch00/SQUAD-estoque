using System;
using System.ComponentModel.DataAnnotations;

namespace SquadEstoque.Web.Models;

public class AjusteEstoqueViewModel
{
    [Required(ErrorMessage = "Selecione o SKU.")]
    public Guid SkuId { get; set; }

    public string ProdutoNome { get; set; } = string.Empty;
    public string Numeracao { get; set; } = string.Empty;
    public int SaldoAtual { get; set; }

    [Required(ErrorMessage = "Informe o novo saldo apurado.")]
    [Range(0, int.MaxValue, ErrorMessage = "O saldo não pode ser negativo.")]
    [Display(Name = "Novo Saldo Apurado")]
    public int NovoSaldoApurado { get; set; }

    [Required(ErrorMessage = "O motivo do ajuste é obrigatório.")]
    [MinLength(5, ErrorMessage = "O motivo deve conter pelo menos 5 caracteres.")]
    [StringLength(500, ErrorMessage = "O motivo não pode exceder 500 caracteres.")]
    [Display(Name = "Motivo do Ajuste")]
    public string Motivo { get; set; } = string.Empty;
}
