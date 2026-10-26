using System;
using System.ComponentModel.DataAnnotations;

namespace SquadEstoque.Web.Models;

public enum StatusAtendimento
{
    PENDENTE = 0,
    CONCLUIDO = 1
}

public enum ResultadoAtendimento
{
    VENDEU = 0,
    NAO_TINHA = 1,
    DESISTIU = 2
}

public class Atendimento
{
    public Guid Id { get; set; }

    [Required]
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    [Required]
    public Guid ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    public Guid? SkuId { get; set; }
    public Sku? Sku { get; set; }

    [Required]
    public StatusAtendimento Status { get; set; } = StatusAtendimento.PENDENTE;

    public ResultadoAtendimento? Resultado { get; set; }
    public int? SaldoConsultado { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? ConcluidoEm { get; set; }
}
