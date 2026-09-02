using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;

namespace SquadEstoque.Web.Controllers;

[Authorize(Roles = "LOJISTA,VENDEDOR")]
public sealed class EstoqueController : Controller
{
    private readonly EstoqueContext _context;

    public EstoqueController(EstoqueContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Consulta(string? termo)
    {
        var model = new ConsultaEstoqueViewModel { Termo = termo?.Trim() ?? string.Empty };

        if (!string.IsNullOrWhiteSpace(model.Termo))
        {
            model.Resultados = await _context.Produto
                .AsNoTracking()
                .Where(p => p.Ativo && p.Nome.Contains(model.Termo))
                .OrderBy(p => p.Nome)
                .Select(p => new ProdutoConsultaResultadoViewModel
                {
                    Id = p.Id,
                    Nome = p.Nome,
                    Marca = p.Marca,
                    Categoria = p.Categoria,
                    Cor = p.Cor
                })
                .ToListAsync();

            if (model.Resultados.Count == 0)
            {
                model.MensagemEstado = "Nenhum produto ativo foi encontrado.";
            }
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(Guid? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var produto = await _context.Produto
            .AsNoTracking()
            .Where(p => p.Id == id && p.Ativo)
            .Select(p => new EstoqueProdutoDetalhesViewModel
            {
                ProdutoId = p.Id,
                Nome = p.Nome,
                Skus = p.Skus
                    .OrderBy(s => s.Numeracao)
                    .Select(s => new EstoqueSkuDetalhesViewModel
                    {
                        Numeracao = s.Numeracao,
                        SaldoAtual = s.SaldoAtual
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync();

        return produto is null ? NotFound() : View(produto);
    }
}
