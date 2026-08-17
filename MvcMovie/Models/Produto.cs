using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MvcMovie.Models;

public class Produto
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Marca { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Categoria { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string Cor { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public ICollection<Sku> Skus { get; set; } = new List<Sku>();
}
