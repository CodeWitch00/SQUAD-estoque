using System.Data.Common;
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
    private readonly ILogger<SaldosZeradosController>? _logger;

    public SaldosZeradosController(EstoqueContext context, ILogger<SaldosZeradosController>? logger = null)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var produtos = await _context.Produto
                .AsNoTracking()
                .Where(produto => produto.Ativo && produto.Skus.Any(sku => sku.Ativo && sku.SaldoAtual == 0))
                .Select(produto => new ProdutoConsultaResultadoViewModel
                {
                    Id = produto.Id,
                    Nome = produto.Nome,
                    Marca = produto.Marca,
                    Cor = produto.Cor,
                    Skus = produto.Skus
                        .Where(sku => sku.Ativo && sku.SaldoAtual == 0)
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
        catch (DbException exception)
        {
            _logger?.LogError(exception, "Falha ao carregar o relatório SaldosZerados.");
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return View("RelatorioIndisponivel");
        }
    }
}
