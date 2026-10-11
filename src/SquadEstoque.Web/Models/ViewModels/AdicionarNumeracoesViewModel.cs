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
    public string? ImagemUrl { get; set; }
    public List<string> NumeracoesAtuais { get; set; } = new();
    public bool CadastroInicial { get; set; }
    public List<GradeNumeracaoItemViewModel> Opcoes { get; set; } = new();
    [Display(Name = "Outras numerações")]
    public string NumeracoesExtras { get; set; } = string.Empty;
}

public class GradeNumeracaoItemViewModel
{
    public string Numeracao { get; set; } = string.Empty;
    public bool Selecionada { get; set; }
    [Range(0, 100000, ErrorMessage = "A quantidade deve ser zero ou maior.")]
    public int Quantidade { get; set; }
}
