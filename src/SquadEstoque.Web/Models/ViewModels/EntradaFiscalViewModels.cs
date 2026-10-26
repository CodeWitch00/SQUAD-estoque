using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SquadEstoque.Web.Models;

public class ImportarEntradaFiscalViewModel
{
    [Required(ErrorMessage = "Selecione o XML da NF-e.")]
    [Display(Name = "XML da NF-e")]
    public IFormFile? ArquivoXml { get; set; }
}

public class ConferirEntradaFiscalViewModel
{
    public Guid EntradaId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string ChaveAcesso { get; set; } = string.Empty;
    public string FornecedorNome { get; set; } = string.Empty;
    public string CnpjFornecedor { get; set; } = string.Empty;
    public StatusEntradaFiscal Status { get; set; }
    public List<SelectListItem> Produtos { get; set; } = new();
    public List<ItemConferenciaEntradaFiscalViewModel> Itens { get; set; } = new();
}

public class ItemConferenciaEntradaFiscalViewModel
{
    public Guid ItemId { get; set; }
    public int NumeroItem { get; set; }
    public string CodigoFornecedor { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }

    [Display(Name = "Produto existente")]
    public Guid? ProdutoId { get; set; }

    [StringLength(200)]
    [Display(Name = "Modelo")]
    public string? NomeModelo { get; set; }

    [StringLength(100)]
    public string? Marca { get; set; }

    [StringLength(100)]
    public string? Categoria { get; set; }

    [StringLength(80)]
    public string? Cor { get; set; }

    [Required(ErrorMessage = "Informe a numeração.")]
    [StringLength(10)]
    [Display(Name = "Numeração")]
    public string Numeracao { get; set; } = string.Empty;

    [Display(Name = "Foto do modelo")]
    public IFormFile? Imagem { get; set; }
    public string? ImagemAtualUrl { get; set; }
    public bool AssociadoAutomaticamente { get; set; }
}
