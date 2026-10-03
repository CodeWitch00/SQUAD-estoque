using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SquadEstoque.Web.Models;

public class AdicionarNumeracoesViewModel
{
    [Required]
    public Guid ProdutoId { get; set; }

    public string NomeProduto { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public List<string> NumeracoesAtuais { get; set; } = new();

    [Required(ErrorMessage = "Informe ao menos uma numeração para adicionar à grade.")]
    [Display(Name = "Novas numerações")]
    public string NumeracoesGrade { get; set; } = string.Empty;
}
