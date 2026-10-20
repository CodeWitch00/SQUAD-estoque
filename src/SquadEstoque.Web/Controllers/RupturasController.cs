using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;

namespace SquadEstoque.Web.Controllers;

[Authorize(Roles = "LOJISTA")]
public sealed class RupturasController(EstoqueContext context) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var rupturas = await context.Ruptura.AsNoTracking()
            .GroupBy(r => new { r.Sku!.ProdutoId, r.Sku.Produto!.Nome,
                r.Sku.Produto.Marca, r.Sku.Produto.Cor, r.Sku.Numeracao })
            .Select(g => new RupturaHistoricoViewModel
            {
                Produto = g.Key.Nome,
                Marca = g.Key.Marca,
                Cor = g.Key.Cor,
                Numeracao = g.Key.Numeracao,
                QuantidadeOcorrencias = g.Count(),
                UltimaOcorrencia = g.Max(r => (DateTime?)r.CriadoEm)
            })
            .OrderByDescending(r => r.QuantidadeOcorrencias)
            .ThenByDescending(r => r.UltimaOcorrencia)
            .ThenBy(r => r.Produto).ThenBy(r => r.Marca)
            .ThenBy(r => r.Cor).ThenBy(r => r.Numeracao)
            .ToListAsync(cancellationToken);

        return View(new RupturasIndexViewModel { Rupturas = rupturas });
    }
}
