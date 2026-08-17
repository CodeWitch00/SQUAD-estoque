using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;

namespace MvcMovie.Controllers;

[Authorize(Roles = "LOJISTA")]
public class ProdutosController : Controller
{
    private readonly EstoqueContext _context;

    public ProdutosController(EstoqueContext context)
    {
        _context = context;
    }

    // GET: /Produtos
    public async Task<IActionResult> Index()
    {
        var produtos = await _context.Produto
            .Include(p => p.Skus)
            .OrderBy(p => p.Nome)
            .ToListAsync();

        return View(produtos);
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

        var tokens = model.NumeracoesGrade
            .Split(new[] { ',', ';', ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();

        if (!tokens.Any())
        {
            ModelState.AddModelError(nameof(model.NumeracoesGrade), "Informe ao menos uma numeração para a grade.");
            return View(model);
        }

        var tamanhos = new List<string>();
        foreach (var token in tokens)
        {
            if (token.Length > 10)
            {
                ModelState.AddModelError(nameof(model.NumeracoesGrade), $"A numeração '{token}' ultrapassa o limite máximo de 10 caracteres.");
                return View(model);
            }

            if (tamanhos.Contains(token, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.NumeracoesGrade), $"A numeração '{token}' foi informada mais de uma vez na grade.");
                return View(model);
            }

            tamanhos.Add(token);
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
