using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;

namespace SquadEstoque.Web.Controllers;

[Authorize(Roles = "LOJISTA")]
public class ProdutosController : Controller
{
    private readonly EstoqueContext _context;
    private readonly IWebHostEnvironment? _environment;

    public ProdutosController(EstoqueContext context, IWebHostEnvironment? environment = null)
    {
        _context = context;
        _environment = environment;
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
        var produtosComHistorico = await ProdutosComHistoricoAsync(produtos);

        return View(new ProdutosIndexViewModel
        {
            Produtos = produtos,
            ProdutosComHistorico = produtosComHistorico,
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

    private async Task<HashSet<Guid>> ProdutosComHistoricoAsync(IReadOnlyCollection<Produto> produtos)
    {
        var produtoIds = produtos.Select(produto => produto.Id).ToArray();
        var skusPorProduto = produtos.ToDictionary(produto => produto.Id, produto => produto.Skus);
        var todosSkuIds = skusPorProduto.Values.SelectMany(skus => skus).Select(sku => sku.Id).ToArray();
        var skuIdsComHistorico = new HashSet<Guid>();
        if (todosSkuIds.Length > 0)
        {
            var idsMovimentados = await _context.Movimentacao.AsNoTracking()
                .Where(item => todosSkuIds.Contains(item.SkuId)).Select(item => item.SkuId).Distinct().ToListAsync();
            var idsRuptura = await _context.Ruptura.AsNoTracking()
                .Where(item => todosSkuIds.Contains(item.SkuId)).Select(item => item.SkuId).Distinct().ToListAsync();
            var idsAtendimentoSku = await _context.Atendimento.AsNoTracking()
                .Where(item => item.SkuId.HasValue && todosSkuIds.Contains(item.SkuId.Value))
                .Select(item => item.SkuId!.Value).Distinct().ToListAsync();
            var idsItemFiscalSku = await _context.ItemEntradaFiscal.AsNoTracking()
                .Where(item => item.SkuId.HasValue && todosSkuIds.Contains(item.SkuId.Value))
                .Select(item => item.SkuId!.Value).Distinct().ToListAsync();
            var idsVinculadosFornecedor = await _context.VinculoProdutoFornecedor.AsNoTracking()
                .Where(item => todosSkuIds.Contains(item.SkuId)).Select(item => item.SkuId).Distinct().ToListAsync();
            skuIdsComHistorico.UnionWith(idsMovimentados);
            skuIdsComHistorico.UnionWith(idsRuptura);
            skuIdsComHistorico.UnionWith(idsAtendimentoSku);
            skuIdsComHistorico.UnionWith(idsItemFiscalSku);
            skuIdsComHistorico.UnionWith(idsVinculadosFornecedor);
        }

        var produtosComAtendimento = await _context.Atendimento.AsNoTracking()
            .Where(item => produtoIds.Contains(item.ProdutoId)).Select(item => item.ProdutoId).Distinct().ToListAsync();
        var produtosEmNfe = await _context.ItemEntradaFiscal.AsNoTracking()
            .Where(item => item.ProdutoId.HasValue && produtoIds.Contains(item.ProdutoId.Value))
            .Select(item => item.ProdutoId!.Value).Distinct().ToListAsync();
        var resultado = new HashSet<Guid>(produtosComAtendimento);
        resultado.UnionWith(produtosEmNfe);
        foreach (var produto in produtos)
        {
            if (produto.Skus.Any(sku => sku.SaldoAtual > 0 || skuIdsComHistorico.Contains(sku.Id)))
                resultado.Add(produto.Id);
        }
        return resultado;
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

        var selecionadas = model.Opcoes.Where(opcao => opcao.Selecionada).ToList();
        var tokens = selecionadas.Select(opcao => opcao.Numeracao)
            .Concat((model.NumeracoesExtras ?? string.Empty).Split(new[] { ',', ';', ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
        if (!TryParseNumeracoes(string.Join(",", tokens), out var numeracoes, out var erroParsing))
        {
            ModelState.AddModelError(string.Empty, erroParsing!);
            return await ReturnAdicionarNumeracoesViewAsync(model);
        }
        if (numeracoes.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Selecione ao menos uma numeração.");
            return await ReturnAdicionarNumeracoesViewAsync(model);
        }
        var quantidades = selecionadas.GroupBy(item => item.Numeracao.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.First().Quantidade, StringComparer.OrdinalIgnoreCase);

        var duplicadasNoFormulario = numeracoes
            .GroupBy(numeracao => numeracao, StringComparer.OrdinalIgnoreCase)
            .Where(grupo => grupo.Count() > 1)
            .Select(grupo => grupo.Key)
            .ToList();

        if (duplicadasNoFormulario.Count > 0)
        {
            ModelState.AddModelError(string.Empty,
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
            ModelState.AddModelError(string.Empty,
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
                ModelState.AddModelError(string.Empty,
                    $"A numeração {string.Join(", ", duplicadasApósReconsulta)} já existe na grade deste produto.");
                return await ReturnAdicionarNumeracoesViewAsync(model);
            }

            var novosSkus = numeracoes.Select(numeracao => new Sku
            {
                Id = Guid.NewGuid(),
                ProdutoId = produtoAtualizado.Id,
                Numeracao = numeracao,
                SaldoAtual = quantidades.GetValueOrDefault(numeracao),
                Ativo = true
            }).ToList();
            _context.Sku.AddRange(novosSkus);
            var usuarioId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idUsuario) ? idUsuario : (Guid?)null;
            var quantidadeTotal = novosSkus.Sum(sku => sku.SaldoAtual);
            if (quantidadeTotal > 0 && usuarioId is null)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Não foi possível identificar o usuário para registrar a entrada de estoque.");
                return await ReturnAdicionarNumeracoesViewAsync(model);
            }
            _context.Movimentacao.AddRange(novosSkus.Where(sku => sku.SaldoAtual > 0).Select(sku => new Movimentacao
            {
                Id = Guid.NewGuid(), SkuId = sku.Id, Tipo = TipoMovimentacao.ENTRADA,
                Quantidade = sku.SaldoAtual, UsuarioId = usuarioId!.Value, Motivo = "Saldo inicial no cadastro da grade"
            }));

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return RedirectToAction(nameof(Details), new { id = produtoAtualizado.Id });
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty,
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
        ViewBag.MarcasSugeridas = _context.Produto.AsNoTracking().Select(p => p.Marca).Distinct().OrderBy(m => m).ToList();
        ViewBag.CategoriasSugeridas = _context.Produto.AsNoTracking().Select(p => p.Categoria).Distinct().OrderBy(c => c).ToList();
        return View();
    }

    // POST: /Produtos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProdutoCreateViewModel model)
    {
        await PopularSugestoesCadastroAsync();
        if (model.Imagem is not null && !await ImagemValidaAsync(model.Imagem))
        {
            ModelState.AddModelError(nameof(model.Imagem), "Use uma imagem JPG, PNG ou WebP de até 2 MB.");
        }
        if (!ModelState.IsValid)
        {
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

        string? arquivoImagem = null;
        if (model.Imagem is not null)
        {
            var imagemSalva = await SalvarImagemAsync(model.Imagem, produto.Id);
            produto.ImagemUrl = imagemSalva.Url;
            arquivoImagem = imagemSalva.Fisico;
        }

        try
        {
            _context.Produto.Add(produto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AdicionarNumeracoes), new { id = produto.Id, cadastroInicial = true });
        }
        catch (DbUpdateException)
        {
            if (arquivoImagem is not null && System.IO.File.Exists(arquivoImagem)) System.IO.File.Delete(arquivoImagem);
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

        var opcoes = submittedModel?.Opcoes ?? Enumerable.Range(34, 11)
            .Select(tamanho => new GradeNumeracaoItemViewModel { Numeracao = tamanho.ToString() }).ToList();
        return new AdicionarNumeracoesViewModel
        {
            ProdutoId = produto.Id,
            NomeProduto = produto.Nome,
            Marca = produto.Marca,
            Categoria = produto.Categoria,
            Cor = produto.Cor,
            ImagemUrl = produto.ImagemUrl,
            CadastroInicial = submittedModel?.CadastroInicial ?? produto.Skus.All(sku => !sku.Ativo),
            Opcoes = opcoes,
            NumeracoesExtras = submittedModel?.NumeracoesExtras ?? string.Empty,
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
            Cor = produto.Cor,
            ImagemAtualUrl = produto.ImagemUrl
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
            model.ImagemAtualUrl = await _context.Produto.AsNoTracking().Where(p => p.Id == id)
                .Select(p => p.ImagemUrl).FirstOrDefaultAsync();
            return View(model);
        }

        if (model.Imagem is not null && !await ImagemValidaAsync(model.Imagem))
        {
            ModelState.AddModelError(nameof(model.Imagem), "Use uma imagem JPG, PNG ou WebP de até 2 MB.");
            model.ImagemAtualUrl = await _context.Produto.AsNoTracking().Where(p => p.Id == id)
                .Select(p => p.ImagemUrl).FirstOrDefaultAsync();
            return View(model);
        }

        var produto = await _context.Produto.FindAsync(id);
        if (produto == null)
        {
            return NotFound();
        }

        var imagemAnterior = produto.ImagemUrl;
        string? arquivoImagem = null;
        produto.Nome = model.Nome.Trim();
        produto.Marca = model.Marca.Trim();
        produto.Categoria = model.Categoria.Trim();
        produto.Cor = model.Cor.Trim();
        if (model.RemoverImagem)
        {
            produto.ImagemUrl = null;
        }
        if (model.Imagem is not null)
        {
            var imagemSalva = await SalvarImagemAsync(model.Imagem, produto.Id);
            produto.ImagemUrl = imagemSalva.Url;
            arquivoImagem = imagemSalva.Fisico;
        }

        try
        {
            await _context.SaveChangesAsync();
            if ((model.RemoverImagem || model.Imagem is not null) && imagemAnterior is not null)
                ExcluirImagemLocal(imagemAnterior);
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            if (arquivoImagem is not null && System.IO.File.Exists(arquivoImagem)) System.IO.File.Delete(arquivoImagem);
            ModelState.AddModelError(string.Empty, "Não foi possível atualizar o produto.");
            model.ImagemAtualUrl = imagemAnterior;
            return View(model);
        }
    }

    private async Task PopularSugestoesCadastroAsync()
    {
        ViewBag.MarcasSugeridas = await _context.Produto.AsNoTracking().Select(p => p.Marca)
            .Distinct().OrderBy(m => m).ToListAsync();
        ViewBag.CategoriasSugeridas = await _context.Produto.AsNoTracking().Select(p => p.Categoria)
            .Distinct().OrderBy(c => c).ToListAsync();
    }

    private static async Task<bool> ImagemValidaAsync(IFormFile imagem)
    {
        if (imagem.Length is <= 0 or > 2 * 1024 * 1024) return false;
        var cabecalho = new byte[12];
        await using var stream = imagem.OpenReadStream();
        var lidos = await stream.ReadAsync(cabecalho);
        return (lidos >= 3 && cabecalho[0] == 0xff && cabecalho[1] == 0xd8 && cabecalho[2] == 0xff) ||
               (lidos >= 8 && cabecalho[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) ||
               (lidos >= 12 && System.Text.Encoding.ASCII.GetString(cabecalho, 0, 4) == "RIFF" &&
                System.Text.Encoding.ASCII.GetString(cabecalho, 8, 4) == "WEBP");
    }

    private async Task<(string Url, string Fisico)> SalvarImagemAsync(IFormFile imagem, Guid produtoId)
    {
        if (_environment?.WebRootPath is null)
            throw new InvalidOperationException("O armazenamento de imagens não está configurado.");

        await using var entrada = imagem.OpenReadStream();
        var cabecalho = new byte[12];
        var lidos = await entrada.ReadAsync(cabecalho);
        entrada.Position = 0;
        var extensao = lidos >= 8 && cabecalho[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })
            ? ".png"
            : lidos >= 12 && System.Text.Encoding.ASCII.GetString(cabecalho, 8, 4) == "WEBP" ? ".webp" : ".jpg";
        var pasta = Path.Combine(_environment.WebRootPath, "uploads", "produtos");
        Directory.CreateDirectory(pasta);
        var nome = $"{produtoId:N}-{Guid.NewGuid():N}{extensao}";
        var fisico = Path.Combine(pasta, nome);
        await using var destino = System.IO.File.Create(fisico);
        await entrada.CopyToAsync(destino);
        return ($"/uploads/produtos/{nome}", fisico);
    }

    private void ExcluirImagemLocal(string url)
    {
        if (_environment?.WebRootPath is null || !url.StartsWith("/uploads/produtos/", StringComparison.Ordinal)) return;
        var nome = Path.GetFileName(url);
        var caminho = Path.Combine(_environment.WebRootPath, "uploads", "produtos", nome);
        if (System.IO.File.Exists(caminho)) System.IO.File.Delete(caminho);
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

    // POST: /Produtos/Excluir/{id}; somente produtos sem estoque nem vínculos históricos.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(Guid id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var produto = await _context.Produto.Include(item => item.Skus)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (produto is null) return NotFound();

        var bloqueados = await ProdutosComHistoricoAsync(new[] { produto });
        if (bloqueados.Contains(produto.Id))
        {
            await transaction.RollbackAsync();
            TempData["MensagemErro"] = "Este produto possui saldo ou histórico associado e não pode ser excluído. Você pode desativá-lo para removê-lo das consultas operacionais.";
            return RedirectToAction(nameof(Index));
        }

        var imagemUrl = produto.ImagemUrl;
        _context.Sku.RemoveRange(produto.Skus);
        _context.Produto.Remove(produto);
        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            if (imagemUrl is not null) ExcluirImagemLocal(imagemUrl);
            TempData["MensagemSucesso"] = "Produto excluído.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            TempData["MensagemErro"] = "O produto não pôde ser excluído porque há registros vinculados. Ele foi mantido para preservar o histórico.";
            return RedirectToAction(nameof(Index));
        }
    }
}
