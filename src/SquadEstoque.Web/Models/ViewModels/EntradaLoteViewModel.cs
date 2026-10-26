using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SquadEstoque.Web.Models;

public class EntradaLoteViewModel
{
    [Required]
    public Guid ProdutoId { get; set; }

    public string NomeProduto { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "A observação não pode exceder 500 caracteres.")]
    public string? Observacao { get; set; }

    public List<EntradaLoteItemViewModel> Itens { get; set; } = new();
}

public class EntradaLoteItemViewModel
{
    public Guid SkuId { get; set; }
    public string Numeracao { get; set; } = string.Empty;
    public int SaldoAtual { get; set; }
    public int? QuantidadeRecebida { get; set; }
}
