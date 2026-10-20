using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;

namespace SquadEstoque.Web.Controllers;

[Authorize(Roles = "LOJISTA")]
public class RupturasController : Controller
{
    private readonly EstoqueContext _context;

    public RupturasController(EstoqueContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var rupturas = await _context.Ruptura
            .AsNoTracking()
            .GroupBy(ruptura => new
            {
                ruptura.SkuId,
                ruptura.Sku!.Numeracao,
                Produto = ruptura.Sku.Produto!.Nome,
                ruptura.Sku.Produto.Marca,
                ruptura.Sku.Produto.Cor
            })
            .Select(grupo => new RupturaHistoricoViewModel
            {
                Produto = grupo.Key.Produto,
                Marca = grupo.Key.Marca,
                Cor = grupo.Key.Cor,
                Numeracao = grupo.Key.Numeracao,
                QuantidadeOcorrencias = grupo.Count(),
                UltimaOcorrencia = grupo.Max(ruptura => ruptura.CriadoEm)
            })
            .ToListAsync();

        return View(new RupturasIndexViewModel { Rupturas = rupturas });
    }
}
