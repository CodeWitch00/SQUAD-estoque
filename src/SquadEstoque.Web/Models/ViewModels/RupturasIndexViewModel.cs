using System;
using System.Collections.Generic;

namespace SquadEstoque.Web.Models;

public class RupturasIndexViewModel
{
    public IReadOnlyList<RupturaHistoricoViewModel> Rupturas { get; set; } =
        new List<RupturaHistoricoViewModel>();
}

/// <summary>
/// Dados de exibição das rupturas agregadas por modelo e numeração.
/// </summary>
public class RupturaHistoricoViewModel
{
    public string Produto { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Cor { get; set; } = string.Empty;
    public string Numeracao { get; set; } = string.Empty;
    public int QuantidadeOcorrencias { get; set; }
    public DateTime? UltimaOcorrencia { get; set; }
}
