using System.ComponentModel.DataAnnotations;

namespace SquadEstoque.Web.Models;

public sealed class AtendimentoVendedorViewModel : IValidatableObject
{
    [Required(ErrorMessage = "Selecione o produto consultado.")]
    public Guid ProdutoId { get; set; }

    [Required(ErrorMessage = "Informe o nome do produto.")]
    [StringLength(200, ErrorMessage = "O nome do produto não pode exceder 200 caracteres.")]
    public string ProdutoNome { get; set; } = string.Empty;

    public Guid? SkuId { get; set; }

    [StringLength(30, ErrorMessage = "A numeração não pode exceder 30 caracteres.")]
    public string? Numeracao { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "O saldo não pode ser negativo.")]
    public int? SaldoAtual { get; set; }

    [EnumDataType(typeof(ResultadoAtendimento))]
    public ResultadoAtendimento? Resultado { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ProdutoId == Guid.Empty)
        {
            yield return new ValidationResult(
                "Selecione o produto consultado.",
                [nameof(ProdutoId)]);
        }

        if (SkuId == Guid.Empty)
        {
            yield return new ValidationResult(
                "Selecione um SKU válido.",
                [nameof(SkuId)]);
        }

        if (Resultado is ResultadoAtendimento.Vendeu or ResultadoAtendimento.NaoTinha &&
            !SkuId.HasValue)
        {
            yield return new ValidationResult(
                "Selecione uma numeração para informar Vendeu ou Não tinha.",
                [nameof(SkuId)]);
        }
    }
}

public enum ResultadoAtendimento
{
    [Display(Name = "Vendeu")]
    Vendeu = 1,

    [Display(Name = "Não tinha")]
    NaoTinha = 2,

    [Display(Name = "Desistiu")]
    Desistiu = 3
}
