using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;

namespace SquadEstoque.Web.Controllers;

[Authorize(Roles = "LOJISTA")]
public class SaldosZeradosController : Controller
{
    private readonly EstoqueContext _context;

    public SaldosZeradosController(EstoqueContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var produtos = await _context.Produto
            .AsNoTracking()
            .Where(produto => produto.Skus.Any(sku => sku.SaldoAtual == 0))
            .Select(produto => new ProdutoConsultaResultadoViewModel
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Marca = produto.Marca,
                Cor = produto.Cor,
                Skus = produto.Skus
                    .Where(sku => sku.SaldoAtual == 0)
                    .Select(sku => new SkuConsultaResultadoViewModel
                    {
                        Id = sku.Id,
                        Numeracao = sku.Numeracao,
                        SaldoAtual = sku.SaldoAtual,
                        UltimaAtualizacaoSaldoEm = sku.UltimaAtualizacaoSaldoEm
                    })
                    .ToList()
            })
            .ToListAsync();

        return View(SaldosZeradosViewModel.Criar(produtos));
    }
}
