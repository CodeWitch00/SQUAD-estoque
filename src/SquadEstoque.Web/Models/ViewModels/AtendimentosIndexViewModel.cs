using System;
using System.Collections.Generic;

namespace SquadEstoque.Web.Models;

public class AtendimentosIndexViewModel
{
    public int TotalPendentes { get; set; }
    public int TotalConcluidos { get; set; }
    public int TotalAlertas { get; set; }
    public List<AtendimentoAuditoriaViewModel> Atendimentos { get; set; } = new();
}

public class AtendimentoAuditoriaViewModel
{
    public Guid Id { get; set; }
    public string Vendedor { get; set; } = string.Empty;
    public string Produto { get; set; } = string.Empty;
    public string? Numeracao { get; set; }
    public int? SaldoConsultado { get; set; }
    public StatusAtendimento Status { get; set; }
    public ResultadoAtendimento? Resultado { get; set; }
    public DateTime CriadoEm { get; set; }
    public DateTime? ConcluidoEm { get; set; }
    public string? Alerta { get; set; }
}
