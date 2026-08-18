using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;

namespace SquadEstoque.Web.Controllers;

public class MovimentacoesController : Controller
{
    private readonly EstoqueContext _context;

    public MovimentacoesController(EstoqueContext context)
    {
        _context = context;
    }

    private Guid? GetAuthenticatedUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(claimValue, out var id))
        {
            return id;
        }
        return null;
    }

    private async Task PopulateSkusDropdownAsync(Guid? selectedSkuId = null)
    {
        var skus = await _context.Sku
            .Include(s => s.Produto)
            .Where(s => s.Produto != null && s.Produto.Ativo)
            .OrderBy(s => s.Produto!.Nome)
            .ThenBy(s => s.Numeracao)
            .Select(s => new
            {
                Id = s.Id,
                Descricao = $"{s.Produto!.Nome} ({s.Produto.Marca} - {s.Produto.Cor}) - Tam. {s.Numeracao} (Saldo: {s.SaldoAtual})"
            })
            .ToListAsync();

        ViewBag.SkusList = new SelectList(skus, "Id", "Descricao", selectedSkuId);
    }

    // ==========================================
    // 1. INDEX — HISTÓRICO DE MOVIMENTAÇÕES
    // ==========================================
    [HttpGet]
    [Authorize(Roles = "LOJISTA")]
    public async Task<IActionResult> Index()
    {
        var movimentacoes = await _context.Movimentacao
            .Include(m => m.Sku)
                .ThenInclude(s => s!.Produto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.CriadoEm)
            .ToListAsync();

        return View(movimentacoes);
    }

    // ==========================================
    // 2. ENTRADA DE ESTOQUE (EXCLUSIVO LOJISTA)
    // ==========================================
    [HttpGet]
    [Authorize(Roles = "LOJISTA")]
    public async Task<IActionResult> Entrada(Guid? skuId)
    {
        var viewModel = new MovimentacaoCreateViewModel
        {
            Tipo = TipoMovimentacao.ENTRADA,
            Quantidade = 1
        };

        if (skuId.HasValue)
        {
            var sku = await _context.Sku
                .Include(s => s.Produto)
                .FirstOrDefaultAsync(s => s.Id == skuId.Value);

            if (sku == null || sku.Produto == null || !sku.Produto.Ativo)
            {
                return NotFound();
            }

            viewModel.SkuId = sku.Id;
            viewModel.ProdutoNome = $"{sku.Produto.Nome} ({sku.Produto.Marca} - {sku.Produto.Cor})";
            viewModel.Numeracao = sku.Numeracao;
            viewModel.SaldoAtual = sku.SaldoAtual;
        }
        else
        {
            await PopulateSkusDropdownAsync();
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "LOJISTA")]
    public async Task<IActionResult> Entrada(MovimentacaoCreateViewModel model)
    {
        var usuarioId = GetAuthenticatedUserId();
        if (!usuarioId.HasValue)
        {
            return Challenge();
        }

        if (model.Quantidade < 1)
        {
            ModelState.AddModelError(nameof(model.Quantidade), "A quantidade de entrada deve ser no mínimo 1.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateSkusDropdownAsync(model.SkuId);
            return View(model);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var sku = await _context.Sku
                .Include(s => s.Produto)
                .FirstOrDefaultAsync(s => s.Id == model.SkuId);

            if (sku == null || sku.Produto == null || !sku.Produto.Ativo)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "O item de estoque selecionado não foi encontrado ou pertence a um produto inativo.");
                await PopulateSkusDropdownAsync(model.SkuId);
                return View(model);
            }

            sku.SaldoAtual += model.Quantidade;

            var movimentacao = new Movimentacao
            {
                Id = Guid.NewGuid(),
                SkuId = sku.Id,
                Tipo = TipoMovimentacao.ENTRADA,
                Quantidade = model.Quantidade,
                UsuarioId = usuarioId.Value,
                CriadoEm = DateTime.UtcNow,
                Motivo = string.IsNullOrWhiteSpace(model.Motivo) ? null : model.Motivo.Trim()
            };

            _context.Movimentacao.Add(movimentacao);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return RedirectToAction("Details", "Produtos", new { id = sku.ProdutoId });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "Erro ao registrar a entrada de estoque. Operação cancelada.");
            await PopulateSkusDropdownAsync(model.SkuId);
            return View(model);
        }
    }

    // ==========================================
    // 3. SAÍDA DE ESTOQUE (LOJISTA E VENDEDOR)
    // ==========================================
    [HttpGet]
    [Authorize(Roles = "LOJISTA,VENDEDOR")]
    public async Task<IActionResult> Saida(Guid? skuId)
    {
        var viewModel = new MovimentacaoCreateViewModel
        {
            Tipo = TipoMovimentacao.SAIDA,
            Quantidade = 1
        };

        if (skuId.HasValue)
        {
            var sku = await _context.Sku
                .Include(s => s.Produto)
                .FirstOrDefaultAsync(s => s.Id == skuId.Value);

            if (sku == null || sku.Produto == null)
            {
                return NotFound();
            }

            viewModel.SkuId = sku.Id;
            viewModel.ProdutoNome = $"{sku.Produto.Nome} ({sku.Produto.Marca} - {sku.Produto.Cor})";
            viewModel.Numeracao = sku.Numeracao;
            viewModel.SaldoAtual = sku.SaldoAtual;
        }
        else
        {
            await PopulateSkusDropdownAsync();
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "LOJISTA,VENDEDOR")]
    public async Task<IActionResult> Saida(MovimentacaoCreateViewModel model)
    {
        var usuarioId = GetAuthenticatedUserId();
        if (!usuarioId.HasValue)
        {
            return Challenge();
        }

        if (model.Quantidade < 1)
        {
            ModelState.AddModelError(nameof(model.Quantidade), "A quantidade de saída deve ser no mínimo 1.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateSkusDropdownAsync(model.SkuId);
            return View(model);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var sku = await _context.Sku
                .Include(s => s.Produto)
                .FirstOrDefaultAsync(s => s.Id == model.SkuId);

            if (sku == null || sku.Produto == null)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "O item de estoque selecionado não foi encontrado.");
                await PopulateSkusDropdownAsync(model.SkuId);
                return View(model);
            }

            if (model.Quantidade > sku.SaldoAtual)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(nameof(model.Quantidade), $"Saldo insuficiente para saída. Saldo disponível: {sku.SaldoAtual} par(es).");
                model.ProdutoNome = $"{sku.Produto.Nome} ({sku.Produto.Marca} - {sku.Produto.Cor})";
                model.Numeracao = sku.Numeracao;
                model.SaldoAtual = sku.SaldoAtual;
                await PopulateSkusDropdownAsync(model.SkuId);
                return View(model);
            }

            sku.SaldoAtual -= model.Quantidade;

            var movimentacao = new Movimentacao
            {
                Id = Guid.NewGuid(),
                SkuId = sku.Id,
                Tipo = TipoMovimentacao.SAIDA,
                Quantidade = model.Quantidade,
                UsuarioId = usuarioId.Value,
                CriadoEm = DateTime.UtcNow,
                Motivo = string.IsNullOrWhiteSpace(model.Motivo) ? null : model.Motivo.Trim()
            };

            _context.Movimentacao.Add(movimentacao);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            if (User.IsInRole("LOJISTA"))
            {
                return RedirectToAction("Details", "Produtos", new { id = sku.ProdutoId });
            }

            return RedirectToAction("Index", "Home");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "O saldo deste item foi alterado por outra operação. Por favor, tente novamente.");
            await PopulateSkusDropdownAsync(model.SkuId);
            return View(model);
        }
    }

    // ==========================================
    // 4. AJUSTE MANUAL (EXCLUSIVO LOJISTA)
    // ==========================================
    [HttpGet]
    [Authorize(Roles = "LOJISTA")]
    public async Task<IActionResult> Ajuste(Guid? skuId)
    {
        var viewModel = new AjusteEstoqueViewModel();

        if (skuId.HasValue)
        {
            var sku = await _context.Sku
                .Include(s => s.Produto)
                .FirstOrDefaultAsync(s => s.Id == skuId.Value);

            if (sku == null || sku.Produto == null)
            {
                return NotFound();
            }

            viewModel.SkuId = sku.Id;
            viewModel.ProdutoNome = $"{sku.Produto.Nome} ({sku.Produto.Marca} - {sku.Produto.Cor})";
            viewModel.Numeracao = sku.Numeracao;
            viewModel.SaldoAtual = sku.SaldoAtual;
            viewModel.NovoSaldoApurado = sku.SaldoAtual;
        }
        else
        {
            await PopulateSkusDropdownAsync();
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "LOJISTA")]
    public async Task<IActionResult> Ajuste(AjusteEstoqueViewModel model)
    {
        var usuarioId = GetAuthenticatedUserId();
        if (!usuarioId.HasValue)
        {
            return Challenge();
        }

        if (model.NovoSaldoApurado < 0)
        {
            ModelState.AddModelError(nameof(model.NovoSaldoApurado), "O novo saldo apurado não pode ser negativo.");
        }

        if (string.IsNullOrWhiteSpace(model.Motivo) || model.Motivo.Trim().Length < 5)
        {
            ModelState.AddModelError(nameof(model.Motivo), "O motivo do ajuste é obrigatório e deve possuir pelo menos 5 caracteres.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateSkusDropdownAsync(model.SkuId);
            return View(model);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var sku = await _context.Sku
                .Include(s => s.Produto)
                .FirstOrDefaultAsync(s => s.Id == model.SkuId);

            if (sku == null || sku.Produto == null)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "O item de estoque selecionado não foi encontrado.");
                await PopulateSkusDropdownAsync(model.SkuId);
                return View(model);
            }

            if (model.NovoSaldoApurado == sku.SaldoAtual)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Não há alteração de saldo a ser registrada.");
                model.ProdutoNome = $"{sku.Produto.Nome} ({sku.Produto.Marca} - {sku.Produto.Cor})";
                model.Numeracao = sku.Numeracao;
                model.SaldoAtual = sku.SaldoAtual;
                await PopulateSkusDropdownAsync(model.SkuId);
                return View(model);
            }

            int diferenca = Math.Abs(model.NovoSaldoApurado - sku.SaldoAtual);
            sku.SaldoAtual = model.NovoSaldoApurado;

            var movimentacao = new Movimentacao
            {
                Id = Guid.NewGuid(),
                SkuId = sku.Id,
                Tipo = TipoMovimentacao.AJUSTE,
                Quantidade = diferenca,
                UsuarioId = usuarioId.Value,
                CriadoEm = DateTime.UtcNow,
                Motivo = model.Motivo.Trim()
            };

            _context.Movimentacao.Add(movimentacao);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return RedirectToAction("Details", "Produtos", new { id = sku.ProdutoId });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "O saldo deste item foi alterado por outra operação. Por favor, tente novamente.");
            await PopulateSkusDropdownAsync(model.SkuId);
            return View(model);
        }
    }
}
