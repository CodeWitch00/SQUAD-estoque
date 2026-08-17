using System;
using System.ComponentModel.DataAnnotations;

namespace MvcMovie.Models;

public enum TipoMovimentacao
{
    ENTRADA,
    SAIDA,
    AJUSTE
}

public class Movimentacao
{
    public Guid Id { get; set; }

    [Required]
    public Guid SkuId { get; set; }
    public Sku? Sku { get; set; }

    [Required]
    public TipoMovimentacao Tipo { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantidade { get; set; }

    [Required]
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public string? Motivo { get; set; }
}
