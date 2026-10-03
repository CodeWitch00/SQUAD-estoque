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
    public async Task<IActionResult> Index(
        string? busca,
        string? tipo,
        DateTime? dataInicio,
        DateTime? dataFim,
        Guid? responsavelId,
        int pagina = 1)
    {
        const int tamanhoPagina = 20;
        busca = string.IsNullOrWhiteSpace(busca) ? null : busca.Trim();
        tipo = NormalizeTipo(tipo);

        var responsaveis = await _context.Movimentacao
            .AsNoTracking()
            .Where(m => m.Usuario != null)
            .Select(m => new MovimentacaoResponsavelOption
            {
                Id = m.UsuarioId,
                Nome = m.Usuario!.Nome
            })
            .Distinct()
            .OrderBy(item => item.Nome)
            .ThenBy(item => item.Id)
            .ToListAsync();

        if (dataInicio.HasValue && dataFim.HasValue && dataInicio.Value.Date > dataFim.Value.Date)
        {
            ModelState.AddModelError(string.Empty, "A data inicial não pode ser posterior à data final.");
            return View(BuildMovimentacoesIndexViewModel(
                new List<Movimentacao>(), responsaveis, busca, tipo, dataInicio, dataFim,
                responsavelId, 1, 0, tamanhoPagina));
        }

        var query = _context.Movimentacao
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = $"%{busca}%";
            query = query.Where(m => m.Sku != null && m.Sku.Produto != null &&
                EF.Functions.Like(m.Sku.Produto.Nome, termo));
        }

        if (tipo != null && Enum.TryParse<TipoMovimentacao>(tipo, true, out var tipoFiltro))
        {
            query = query.Where(m => m.Tipo == tipoFiltro);
        }

        if (dataInicio.HasValue)
        {
            var inicio = dataInicio.Value.Date;
            query = query.Where(m => m.CriadoEm >= inicio);
        }

        if (dataFim.HasValue)
        {
            var fimExclusivo = dataFim.Value.Date.AddDays(1);
            query = query.Where(m => m.CriadoEm < fimExclusivo);
        }

        if (responsavelId.HasValue)
        {
            query = query.Where(m => m.UsuarioId == responsavelId.Value);
        }

        var totalItens = await query.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(totalItens / (double)tamanhoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var movimentacoes = await query
            .Include(m => m.Sku)
                .ThenInclude(s => s!.Produto)
            .Include(m => m.Usuario)
            .OrderByDescending(m => m.CriadoEm)
            .ThenByDescending(m => m.Id)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();

        return View(BuildMovimentacoesIndexViewModel(
            movimentacoes, responsaveis, busca, tipo, dataInicio, dataFim,
            responsavelId, pagina, totalItens, tamanhoPagina, totalPaginas));
    }

    private static string? NormalizeTipo(string? tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
        {
            return null;
        }

        return Enum.TryParse<TipoMovimentacao>(tipo.Trim(), true, out var parsed)
            ? parsed.ToString()
            : null;
    }

    private static MovimentacoesIndexViewModel BuildMovimentacoesIndexViewModel(
        IReadOnlyList<Movimentacao> movimentacoes,
        IReadOnlyList<MovimentacaoResponsavelOption> responsaveis,
        string? busca,
        string? tipo,
        DateTime? dataInicio,
        DateTime? dataFim,
        Guid? responsavelId,
        int pagina,
        int totalItens,
        int tamanhoPagina,
        int? totalPaginas = null)
    {
        return new MovimentacoesIndexViewModel
        {
            Movimentacoes = movimentacoes,
            Responsaveis = responsaveis,
            Busca = busca,
            Tipo = tipo,
            DataInicio = dataInicio,
            DataFim = dataFim,
            ResponsavelId = responsavelId,
            PaginaAtual = pagina,
            TotalPaginas = totalPaginas ?? Math.Max(1, (int)Math.Ceiling(totalItens / (double)tamanhoPagina)),
            TotalItens = totalItens,
            TamanhoPagina = tamanhoPagina
        };
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
    // 2A. ENTRADA DE ESTOQUE EM LOTE (EXCLUSIVO LOJISTA)
    // ==========================================
    [HttpGet]
    [Authorize(Roles = "LOJISTA")]
    public async Task<IActionResult> EntradaLote(Guid produtoId)
    {
        var viewModel = await BuildEntradaLoteViewModelAsync(produtoId);
        return viewModel == null ? NotFound() : View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "LOJISTA")]
    public async Task<IActionResult> EntradaLote(EntradaLoteViewModel model)
    {
        var usuarioId = GetAuthenticatedUserId();
        if (!usuarioId.HasValue)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return await ReturnEntradaLoteViewAsync(model);
        }

        var produto = await _context.Produto
            .Include(p => p.Skus)
            .FirstOrDefaultAsync(p => p.Id == model.ProdutoId && p.Ativo);

        if (produto == null)
        {
            ModelState.AddModelError(string.Empty, "O produto selecionado não foi encontrado ou está inativo.");
            return await ReturnEntradaLoteViewAsync(model);
        }

        var itens = model.Itens ?? new List<EntradaLoteItemViewModel>();
        var itensComId = itens.Where(item => item.SkuId != Guid.Empty).ToList();
        var idsDuplicados = itensComId
            .GroupBy(item => item.SkuId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (idsDuplicados.Count > 0)
        {
            ModelState.AddModelError(string.Empty, "A mesma numeração não pode ser enviada mais de uma vez.");
        }

        var itensPositivos = new List<EntradaLoteItemViewModel>();
        foreach (var (item, index) in itens.Select((item, index) => (item, index)))
        {
            var quantidade = item.QuantidadeRecebida.GetValueOrDefault();
            if (quantidade < 0)
            {
                ModelState.AddModelError($"Itens[{index}].QuantidadeRecebida", "Informe uma quantidade positiva ou deixe o campo vazio.");
            }
            else if (quantidade > 0)
            {
                if (item.SkuId == Guid.Empty)
                {
                    ModelState.AddModelError(string.Empty, "Uma das numerações informadas não foi identificada.");
                }
                else
                {
                    itensPositivos.Add(item);
                }
            }
        }

        if (itensPositivos.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Informe a quantidade recebida de pelo menos uma numeração.");
        }

        var idsEnviados = itensComId.Select(item => item.SkuId).Distinct().ToList();
        var skusEnviados = await _context.Sku
            .Where(sku => idsEnviados.Contains(sku.Id))
            .ToListAsync();
        var skusPorId = skusEnviados.ToDictionary(sku => sku.Id);

        foreach (var item in itensComId)
        {
            if (!skusPorId.TryGetValue(item.SkuId, out var sku))
            {
                ModelState.AddModelError(string.Empty, "Uma das numerações informadas não foi encontrada.");
            }
            else if (sku.ProdutoId != model.ProdutoId || !sku.Ativo)
            {
                ModelState.AddModelError(string.Empty, "Todas as numerações devem pertencer ao produto ativo selecionado.");
            }
        }

        foreach (var item in itensPositivos)
        {
            if (skusPorId.TryGetValue(item.SkuId, out var sku) &&
                (long)sku.SaldoAtual + item.QuantidadeRecebida!.Value > int.MaxValue)
            {
                ModelState.AddModelError(string.Empty, "A quantidade informada excede o limite de estoque permitido.");
            }
        }

        if (!ModelState.IsValid)
        {
            return await ReturnEntradaLoteViewAsync(model);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var idsPositivos = itensPositivos.Select(item => item.SkuId).ToList();
            var skusParaAtualizar = await _context.Sku
                .Where(sku => idsPositivos.Contains(sku.Id))
                .ToListAsync();

            if (skusParaAtualizar.Count != idsPositivos.Count ||
                skusParaAtualizar.Any(sku => sku.ProdutoId != model.ProdutoId || !sku.Ativo))
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Uma das numerações não está mais disponível para entrada.");
                return await ReturnEntradaLoteViewAsync(model);
            }

            var skusParaAtualizarPorId = skusParaAtualizar.ToDictionary(sku => sku.Id);
            var criadoEm = DateTime.UtcNow;
            var observacao = string.IsNullOrWhiteSpace(model.Observacao) ? null : model.Observacao.Trim();

            foreach (var item in itensPositivos)
            {
                var sku = skusParaAtualizarPorId[item.SkuId];
                var quantidade = item.QuantidadeRecebida!.Value;
                if ((long)sku.SaldoAtual + quantidade > int.MaxValue)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "A quantidade informada excede o limite de estoque permitido.");
                    return await ReturnEntradaLoteViewAsync(model);
                }

                sku.SaldoAtual += quantidade;
                _context.Movimentacao.Add(new Movimentacao
                {
                    Id = Guid.NewGuid(),
                    SkuId = sku.Id,
                    Tipo = TipoMovimentacao.ENTRADA,
                    Quantidade = quantidade,
                    UsuarioId = usuarioId.Value,
                    CriadoEm = criadoEm,
                    Motivo = observacao
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return RedirectToAction("Details", "Produtos", new { id = model.ProdutoId });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "Erro ao registrar a entrada em lote. Operação cancelada.");
            return await ReturnEntradaLoteViewAsync(model);
        }
    }

    private async Task<EntradaLoteViewModel?> BuildEntradaLoteViewModelAsync(Guid produtoId,
        EntradaLoteViewModel? submittedModel = null)
    {
        var produto = await _context.Produto
            .Include(p => p.Skus)
            .FirstOrDefaultAsync(p => p.Id == produtoId && p.Ativo);

        if (produto == null)
        {
            return null;
        }

        var submittedQuantities = submittedModel?.Itens
            .GroupBy(item => item.SkuId)
            .ToDictionary(group => group.Key, group => group.Last().QuantidadeRecebida)
            ?? new Dictionary<Guid, int?>();

        return new EntradaLoteViewModel
        {
            ProdutoId = produto.Id,
            NomeProduto = produto.Nome,
            Marca = produto.Marca,
            Categoria = produto.Categoria,
            Cor = produto.Cor,
            Observacao = submittedModel?.Observacao,
            Itens = produto.Skus
                .Where(sku => sku.Ativo)
                .OrderBy(sku => sku.Numeracao)
                .Select(sku => new EntradaLoteItemViewModel
                {
                    SkuId = sku.Id,
                    Numeracao = sku.Numeracao,
                    SaldoAtual = sku.SaldoAtual,
                    QuantidadeRecebida = submittedQuantities.GetValueOrDefault(sku.Id)
                })
                .ToList()
        };
    }

    private async Task<IActionResult> ReturnEntradaLoteViewAsync(EntradaLoteViewModel model)
    {
        var viewModel = await BuildEntradaLoteViewModelAsync(model.ProdutoId, model);
        if (viewModel == null)
        {
            return NotFound();
        }

        return View("EntradaLote", viewModel);
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

        try
        {
            var resultado = await _context.RegistrarSaidaAsync(
                model.SkuId, model.Quantidade, usuarioId.Value, model.Motivo);

            if (resultado.Erro != null)
            {
                ModelState.AddModelError(resultado.Campo, resultado.Erro);
                if (resultado.Sku is { } sku)
                {
                    model.ProdutoNome = $"{sku.Produto!.Nome} ({sku.Produto.Marca} - {sku.Produto.Cor})";
                    model.Numeracao = sku.Numeracao;
                    model.SaldoAtual = sku.SaldoAtual;
                }
                await PopulateSkusDropdownAsync(model.SkuId);
                return View(model);
            }

            if (User.IsInRole("LOJISTA"))
            {
                return RedirectToAction("Details", "Produtos", new { id = resultado.Sku!.ProdutoId });
            }

            return RedirectToAction("Index", "Home");
        }
        catch (Exception)
        {
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
