using System;
using System.ComponentModel.DataAnnotations;

namespace SquadEstoque.Web.Models;

public enum PerfilUsuario
{
    VENDEDOR,
    LOJISTA
}

public class Usuario
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string SenhaHash { get; set; } = string.Empty;

    [Required]
    public PerfilUsuario Perfil { get; set; }
}
