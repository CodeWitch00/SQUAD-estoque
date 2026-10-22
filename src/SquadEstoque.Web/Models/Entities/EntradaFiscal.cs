using System.ComponentModel.DataAnnotations;

namespace SquadEstoque.Web.Models;

public enum StatusEntradaFiscal
{
    RASCUNHO = 0,
    CONFIRMADA = 1
}

public class EntradaFiscal
{
    public Guid Id { get; set; }

    [Required, StringLength(44, MinimumLength = 44)]
    public string ChaveAcesso { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Numero { get; set; } = string.Empty;

    [Required, StringLength(14)]
    public string CnpjFornecedor { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string FornecedorNome { get; set; } = string.Empty;

    public StatusEntradaFiscal Status { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmadoEm { get; set; }
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public ICollection<ItemEntradaFiscal> Itens { get; set; } = new List<ItemEntradaFiscal>();
}

public class ItemEntradaFiscal
{
    public Guid Id { get; set; }
    public Guid EntradaFiscalId { get; set; }
    public EntradaFiscal? EntradaFiscal { get; set; }
    public int NumeroItem { get; set; }

    [Required, StringLength(60)]
    public string CodigoFornecedor { get; set; } = string.Empty;

    [StringLength(14)]
    public string? Gtin { get; set; }

    [Required, StringLength(120)]
    public string Descricao { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantidade { get; set; }

    public decimal ValorUnitario { get; set; }
    public Guid? ProdutoId { get; set; }
    public Produto? Produto { get; set; }
    public Guid? SkuId { get; set; }
    public Sku? Sku { get; set; }
}

public class VinculoProdutoFornecedor
{
    public Guid Id { get; set; }

    [Required, StringLength(14)]
    public string CnpjFornecedor { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string CodigoFornecedor { get; set; } = string.Empty;

    public Guid SkuId { get; set; }
    public Sku? Sku { get; set; }
}
