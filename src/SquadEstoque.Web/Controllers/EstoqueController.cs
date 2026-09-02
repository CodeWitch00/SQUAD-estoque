using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;

namespace SquadEstoque.Web.Controllers;

[Authorize(Roles = "VENDEDOR")]
public class EstoqueController : Controller
{
    private const int TamanhoMinimoTermo = 2;
    private const int TamanhoMaximoTermo = 100;
    private readonly EstoqueContext _context;

    public EstoqueController(EstoqueContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Consulta(string? termo, Guid? produtoId)
    {
        var viewModel = new ConsultaEstoqueViewModel
        {
            Termo = termo?.Trim() ?? string.Empty
        };

        if (viewModel.Termo.Length == 0)
        {
            return View(viewModel);
        }

        if (viewModel.Termo.Length < TamanhoMinimoTermo)
        {
            ModelState.AddModelError(
                nameof(ConsultaEstoqueViewModel.Termo),
                "Digite pelo menos 2 caracteres para buscar.");
            return View(viewModel);
        }

        if (viewModel.Termo.Length > TamanhoMaximoTermo)
        {
            ModelState.AddModelError(
                nameof(ConsultaEstoqueViewModel.Termo),
                "A busca deve ter no máximo 100 caracteres.");
            return View(viewModel);
        }

        var padrao = $"%{EscapeLikePattern(viewModel.Termo)}%";

        viewModel.Resultados = await _context.Produto
            .AsNoTracking()
            .Where(produto => produto.Ativo &&
                (EF.Functions.Like(produto.Nome, padrao, "\\") ||
                 EF.Functions.Like(produto.Marca, padrao, "\\") ||
                 EF.Functions.Like(produto.Categoria, padrao, "\\") ||
                 EF.Functions.Like(produto.Cor, padrao, "\\")))
            .OrderBy(produto => produto.Nome)
            .ThenBy(produto => produto.Marca)
            .ThenBy(produto => produto.Cor)
            .Select(produto => new ProdutoConsultaResultadoViewModel
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Marca = produto.Marca,
                Categoria = produto.Categoria,
                Cor = produto.Cor,
                Skus = produto.Skus
                    .Where(sku => sku.Ativo)
                    .OrderBy(sku => sku.Numeracao)
                    .Select(sku => new SkuConsultaResultadoViewModel
                    {
                        Id = sku.Id,
                        Numeracao = sku.Numeracao,
                        SaldoAtual = sku.SaldoAtual
                    })
                    .ToList()
            })
            .ToListAsync();

        if (viewModel.Resultados.Count == 0)
        {
            viewModel.MensagemEstado = "Produto não encontrado.";
            return View(viewModel);
        }

        if (produtoId.HasValue && viewModel.Resultados.Any(produto => produto.Id == produtoId.Value))
        {
            viewModel.ProdutoSelecionadoId = produtoId;
        }

        return View(viewModel);
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}
