using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;

namespace SquadEstoque.Web.Controllers;

[Authorize(Roles = "LOJISTA")]
public class ProdutosController : Controller
{
    private readonly EstoqueContext _context;

    public ProdutosController(EstoqueContext context)
    {
        _context = context;
    }

    // GET: /Produtos
    public async Task<IActionResult> Index(
        string? busca,
        string? status,
        string? marca,
        string? categoria,
        int pagina = 1)
    {
        const int tamanhoPagina = 20;
        busca = string.IsNullOrWhiteSpace(busca) ? null : busca.Trim();
        status = status?.Trim().ToLowerInvariant() switch
        {
            "ativo" => "ativo",
            "inativo" => "inativo",
            _ => null
        };
        marca = string.IsNullOrWhiteSpace(marca) ? null : marca.Trim();
        categoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria.Trim();

        var query = _context.Produto
            .AsNoTracking()
            .Include(p => p.Skus)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = $"%{busca}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Nome, termo) ||
                EF.Functions.Like(p.Marca, termo));
        }

        if (status == "ativo")
        {
            query = query.Where(p => p.Ativo);
        }
        else if (status == "inativo")
        {
            query = query.Where(p => !p.Ativo);
        }

        if (!string.IsNullOrWhiteSpace(marca))
        {
            query = query.Where(p => p.Marca == marca);
        }

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            query = query.Where(p => p.Categoria == categoria);
        }

        var marcas = await _context.Produto
            .AsNoTracking()
            .Select(p => p.Marca)
            .Where(valor => valor != "")
            .Distinct()
            .OrderBy(valor => valor)
            .ToListAsync();
        var categorias = await _context.Produto
            .AsNoTracking()
            .Select(p => p.Categoria)
            .Where(valor => valor != "")
            .Distinct()
            .OrderBy(valor => valor)
            .ToListAsync();

        var totalItens = await query.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalItens / (double)tamanhoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var produtos = await query
            .OrderBy(p => p.Nome)
            .ThenBy(p => p.Id)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return View(new ProdutosIndexViewModel
        {
            Produtos = produtos,
            Marcas = marcas,
            Categorias = categorias,
            Busca = busca,
            Status = status,
            Marca = marca,
            Categoria = categoria,
            PaginaAtual = pagina,
            TotalPaginas = totalPaginas,
            TotalItens = totalItens,
            TamanhoPagina = tamanhoPagina
        });
    }

    // GET: /Produtos/Details/{id}
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var produto = await _context.Produto
            .Include(p => p.Skus)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produto == null)
        {
            return NotFound();
        }

        return View(produto);
    }

    // GET: /Produtos/AdicionarNumeracoes/{id}
    [HttpGet]
    public async Task<IActionResult> AdicionarNumeracoes(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var viewModel = await BuildAdicionarNumeracoesViewModelAsync(id.Value, somenteAtivo: true);
        return viewModel == null ? NotFound() : View(viewModel);
    }

    // POST: /Produtos/AdicionarNumeracoes/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdicionarNumeracoes(Guid id, AdicionarNumeracoesViewModel model)
    {
        if (id != model.ProdutoId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return await ReturnAdicionarNumeracoesViewAsync(model);
        }

        if (!TryParseNumeracoes(model.NumeracoesGrade, out var numeracoes, out var erroParsing))
        {
            ModelState.AddModelError(nameof(model.NumeracoesGrade), erroParsing!);
            return await ReturnAdicionarNumeracoesViewAsync(model);
        }

        var duplicadasNoFormulario = numeracoes
            .GroupBy(numeracao => numeracao, StringComparer.OrdinalIgnoreCase)
            .Where(grupo => grupo.Count() > 1)
            .Select(grupo => grupo.Key)
            .ToList();

        if (duplicadasNoFormulario.Count > 0)
        {
            ModelState.AddModelError(nameof(model.NumeracoesGrade),
                $"A numeração {string.Join(", ", duplicadasNoFormulario)} foi informada mais de uma vez.");
            return await ReturnAdicionarNumeracoesViewAsync(model);
        }

        var produto = await _context.Produto
            .Include(p => p.Skus)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (produto == null)
        {
            return NotFound();
        }

        if (!produto.Ativo)
        {
            ModelState.AddModelError(string.Empty, "Não é possível adicionar numerações a um produto inativo.");
            return await ReturnAdicionarNumeracoesViewAsync(model);
        }

        var duplicadasNaGrade = numeracoes
            .Where(numeracao => produto.Skus.Any(sku =>
                string.Equals(sku.Numeracao, numeracao, StringComparison.OrdinalIgnoreCase)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (duplicadasNaGrade.Count > 0)
        {
            ModelState.AddModelError(nameof(model.NumeracoesGrade),
                $"A numeração {string.Join(", ", duplicadasNaGrade)} já existe na grade deste produto.");
            return await ReturnAdicionarNumeracoesViewAsync(model);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var produtoAtualizado = await _context.Produto
                .Include(p => p.Skus)
                .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);

            if (produtoAtualizado == null)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "O produto não está disponível para atualização.");
                return await ReturnAdicionarNumeracoesViewAsync(model);
            }

            var duplicadasApósReconsulta = numeracoes
                .Where(numeracao => produtoAtualizado.Skus.Any(sku =>
                    string.Equals(sku.Numeracao, numeracao, StringComparison.OrdinalIgnoreCase)))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (duplicadasApósReconsulta.Count > 0)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(nameof(model.NumeracoesGrade),
                    $"A numeração {string.Join(", ", duplicadasApósReconsulta)} já existe na grade deste produto.");
                return await ReturnAdicionarNumeracoesViewAsync(model);
            }

            _context.Sku.AddRange(numeracoes.Select(numeracao => new Sku
            {
                Id = Guid.NewGuid(),
                ProdutoId = produtoAtualizado.Id,
                Numeracao = numeracao,
                SaldoAtual = 0,
                Ativo = true
            }));

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return RedirectToAction(nameof(Details), new { id = produtoAtualizado.Id });
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(nameof(model.NumeracoesGrade),
                "Não foi possível adicionar as numerações. Verifique se alguma já existe na grade.");
            return await ReturnAdicionarNumeracoesViewAsync(model);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "Erro ao adicionar as numerações. Operação cancelada.");
            return await ReturnAdicionarNumeracoesViewAsync(model);
        }
    }

    // GET: /Produtos/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Produtos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProdutoCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!TryParseNumeracoes(model.NumeracoesGrade, out var tamanhos, out var erroParsing))
        {
            ModelState.AddModelError(nameof(model.NumeracoesGrade), erroParsing!);
            return View(model);
        }

        var produto = new Produto
        {
            Id = Guid.NewGuid(),
            Nome = model.Nome.Trim(),
            Marca = model.Marca.Trim(),
            Categoria = model.Categoria.Trim(),
            Cor = model.Cor.Trim(),
            Ativo = true
        };

        foreach (var tamanho in tamanhos)
        {
            produto.Skus.Add(new Sku
            {
                Id = Guid.NewGuid(),
                ProdutoId = produto.Id,
                Numeracao = tamanho,
                SaldoAtual = 0,
                Ativo = true
            });
        }

        try
        {
            _context.Produto.Add(produto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Erro ao salvar o produto no banco de dados. Verifique os dados informados.");
            return View(model);
        }
    }

    private static bool TryParseNumeracoes(string? valor, out List<string> numeracoes, out string? erro)
    {
        numeracoes = valor?
            .Split(new[] { ',', ';', ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(token => token.Trim())
            .Where(token => !string.IsNullOrEmpty(token))
            .ToList() ?? new List<string>();
        erro = null;

        if (numeracoes.Count == 0)
        {
            erro = "Informe ao menos uma numeração para a grade.";
            return false;
        }

        foreach (var numeracao in numeracoes)
        {
            if (numeracao.Length > 10)
            {
                erro = $"A numeração '{numeracao}' ultrapassa o limite máximo de 10 caracteres.";
                return false;
            }

            if (numeracoes.Count(n => string.Equals(n, numeracao, StringComparison.OrdinalIgnoreCase)) > 1)
            {
                erro = $"A numeração '{numeracao}' foi informada mais de uma vez na grade.";
                return false;
            }
        }

        return true;
    }

    private async Task<AdicionarNumeracoesViewModel?> BuildAdicionarNumeracoesViewModelAsync(
        Guid produtoId, AdicionarNumeracoesViewModel? submittedModel = null, bool somenteAtivo = false)
    {
        var consulta = _context.Produto
            .Include(p => p.Skus)
            .AsQueryable();

        if (somenteAtivo)
        {
            consulta = consulta.Where(p => p.Ativo);
        }

        var produto = await consulta.FirstOrDefaultAsync(p => p.Id == produtoId);

        if (produto == null)
        {
            return null;
        }

        return new AdicionarNumeracoesViewModel
        {
            ProdutoId = produto.Id,
            NomeProduto = produto.Nome,
            Marca = produto.Marca,
            Categoria = produto.Categoria,
            Cor = produto.Cor,
            NumeracoesGrade = submittedModel?.NumeracoesGrade ?? string.Empty,
            NumeracoesAtuais = produto.Skus
                .Where(sku => sku.Ativo)
                .OrderBy(sku => sku.Numeracao)
                .Select(sku => sku.Numeracao)
                .ToList()
        };
    }

    private async Task<IActionResult> ReturnAdicionarNumeracoesViewAsync(AdicionarNumeracoesViewModel model)
    {
        var viewModel = await BuildAdicionarNumeracoesViewModelAsync(model.ProdutoId, model);
        if (viewModel == null)
        {
            return NotFound();
        }

        return View("AdicionarNumeracoes", viewModel);
    }

    // GET: /Produtos/Edit/{id}
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var produto = await _context.Produto.FindAsync(id);
        if (produto == null)
        {
            return NotFound();
        }

        var viewModel = new ProdutoEditViewModel
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Marca = produto.Marca,
            Categoria = produto.Categoria,
            Cor = produto.Cor
        };

        return View(viewModel);
    }

    // POST: /Produtos/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ProdutoEditViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var produto = await _context.Produto.FindAsync(id);
        if (produto == null)
        {
            return NotFound();
        }

        produto.Nome = model.Nome.Trim();
        produto.Marca = model.Marca.Trim();
        produto.Categoria = model.Categoria.Trim();
        produto.Cor = model.Cor.Trim();

        try
        {
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "Não foi possível atualizar o produto.");
            return View(model);
        }
    }

    // POST: /Produtos/ToggleAtivo/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAtivo(Guid id)
    {
        var produto = await _context.Produto.FindAsync(id);
        if (produto == null)
        {
            return NotFound();
        }

        produto.Ativo = !produto.Ativo;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
