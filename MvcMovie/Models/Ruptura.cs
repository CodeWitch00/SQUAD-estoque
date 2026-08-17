using System;
using System.ComponentModel.DataAnnotations;

namespace MvcMovie.Models;

public class Ruptura
{
    public Guid Id { get; set; }

    [Required]
    public Guid SkuId { get; set; }
    public Sku? Sku { get; set; }

    [Required]
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
