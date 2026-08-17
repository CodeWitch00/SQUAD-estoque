using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MvcMovie.Models;

public class Sku
{
    public Guid Id { get; set; }

    [Required]
    public Guid ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    [Required]
    [StringLength(10)]
    public string Numeracao { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int SaldoAtual { get; set; } = 0;

    public bool Ativo { get; set; } = true;

    public ICollection<Movimentacao> Movimentacoes { get; set; } = new List<Movimentacao>();
    public ICollection<Ruptura> Rupturas { get; set; } = new List<Ruptura>();
}
