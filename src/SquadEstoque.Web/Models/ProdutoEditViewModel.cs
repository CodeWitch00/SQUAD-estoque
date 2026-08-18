using System;
using System.ComponentModel.DataAnnotations;

namespace SquadEstoque.Web.Models;

public class ProdutoEditViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do modelo")]
    [StringLength(200, ErrorMessage = "O nome não pode exceder 200 caracteres")]
    [Display(Name = "Nome do Modelo")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a marca")]
    [StringLength(100, ErrorMessage = "A marca não pode exceder 100 caracteres")]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a categoria")]
    [StringLength(100, ErrorMessage = "A categoria não pode exceder 100 caracteres")]
    public string Categoria { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a cor")]
    [StringLength(80, ErrorMessage = "A cor não pode exceder 80 caracteres")]
    public string Cor { get; set; } = string.Empty;
}
