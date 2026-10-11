using System.Globalization;
using System.Security.Claims;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;

namespace SquadEstoque.Web.Controllers;

[Authorize(Roles = "LOJISTA")]
public sealed class EntradasFiscaisController : Controller
{
    private const long TamanhoMaximoXml = 5 * 1024 * 1024;
    private const long TamanhoMaximoImagem = 2 * 1024 * 1024;
    private readonly EstoqueContext _context;
    private readonly IWebHostEnvironment _environment;

    public EntradasFiscaisController(EstoqueContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var entradas = await _context.EntradaFiscal.AsNoTracking()
            .Include(e => e.Itens)
            .OrderByDescending(e => e.CriadoEm)
            .ToListAsync();
        return View(entradas);
    }

    [HttpGet]
    public IActionResult Importar() => View(new ImportarEntradaFiscalViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Importar(ImportarEntradaFiscalViewModel model)
    {
        var usuarioId = UsuarioId();
        if (!usuarioId.HasValue) return Challenge();

        var arquivo = model.ArquivoXml;
        if (arquivo is null || arquivo.Length == 0)
            ModelState.AddModelError(nameof(model.ArquivoXml), "Selecione o XML da NF-e.");
        else if (arquivo.Length > TamanhoMaximoXml)
            ModelState.AddModelError(nameof(model.ArquivoXml), "O XML deve ter no máximo 5 MB.");
        else if (!string.Equals(Path.GetExtension(arquivo.FileName), ".xml", StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(model.ArquivoXml), "Envie um arquivo XML.");

        if (!ModelState.IsValid) return View(model);

        DadosNfe dados;
        try
        {
            dados = await LerNfeAsync(arquivo!);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(nameof(model.ArquivoXml), exception.Message);
            return View(model);
        }
        catch (XmlException)
        {
            ModelState.AddModelError(nameof(model.ArquivoXml), "O arquivo não contém um XML válido.");
            return View(model);
        }

        var existente = await _context.EntradaFiscal.AsNoTracking()
            .FirstOrDefaultAsync(e => e.ChaveAcesso == dados.ChaveAcesso);
        if (existente is not null)
        {
            ModelState.AddModelError(nameof(model.ArquivoXml),
                existente.Status == StatusEntradaFiscal.CONFIRMADA
                    ? "Esta NF-e já foi confirmada e não pode ser importada novamente."
                    : "Esta NF-e já está em conferência.");
            return View(model);
        }

        var entrada = new EntradaFiscal
        {
            Id = Guid.NewGuid(),
            ChaveAcesso = dados.ChaveAcesso,
            Numero = dados.Numero,
            CnpjFornecedor = dados.CnpjFornecedor,
            FornecedorNome = dados.FornecedorNome,
            Status = StatusEntradaFiscal.RASCUNHO,
            UsuarioId = usuarioId.Value,
            CriadoEm = DateTime.UtcNow,
            Itens = dados.Itens.Select(item => new ItemEntradaFiscal
            {
                Id = Guid.NewGuid(),
                NumeroItem = item.NumeroItem,
                CodigoFornecedor = item.CodigoFornecedor,
                Gtin = item.Gtin,
                Descricao = item.Descricao,
                Quantidade = item.Quantidade,
                ValorUnitario = item.ValorUnitario
            }).ToList()
        };
        _context.EntradaFiscal.Add(entrada);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(model.ArquivoXml),
                "Esta NF-e já foi importada por outra operação.");
            return View(model);
        }
        return RedirectToAction(nameof(Conferir), new { id = entrada.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Conferir(Guid id)
    {
        var model = await MontarConferenciaAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirmar(ConferirEntradaFiscalViewModel model)
    {
        var usuarioId = UsuarioId();
        if (!usuarioId.HasValue) return Challenge();

        var entrada = await _context.EntradaFiscal.Include(e => e.Itens)
            .FirstOrDefaultAsync(e => e.Id == model.EntradaId);
        if (entrada is null) return NotFound();
        if (entrada.Status != StatusEntradaFiscal.RASCUNHO)
        {
            ModelState.AddModelError(string.Empty, "Esta entrada já foi confirmada.");
            return await RetornarConferenciaAsync(model, entrada);
        }

        if (model.Itens.Count != entrada.Itens.Count ||
            model.Itens.Select(i => i.ItemId).Distinct().Count() != entrada.Itens.Count ||
            model.Itens.Any(i => entrada.Itens.All(e => e.Id != i.ItemId)))
            return BadRequest();

        foreach (var item in model.Itens)
        {
            item.Numeracao = item.Numeracao?.Trim() ?? string.Empty;
            if (!item.ProdutoId.HasValue)
            {
                if (string.IsNullOrWhiteSpace(item.NomeModelo))
                    ModelState.AddModelError($"Itens[{model.Itens.IndexOf(item)}].NomeModelo", "Informe o modelo.");
                if (string.IsNullOrWhiteSpace(item.Marca))
                    ModelState.AddModelError($"Itens[{model.Itens.IndexOf(item)}].Marca", "Informe a marca.");
                if (string.IsNullOrWhiteSpace(item.Categoria))
                    ModelState.AddModelError($"Itens[{model.Itens.IndexOf(item)}].Categoria", "Informe a categoria.");
                if (string.IsNullOrWhiteSpace(item.Cor))
                    ModelState.AddModelError($"Itens[{model.Itens.IndexOf(item)}].Cor", "Informe a cor.");
            }
            if (item.Imagem is not null && !await ImagemValidaAsync(item.Imagem))
                ModelState.AddModelError($"Itens[{model.Itens.IndexOf(item)}].Imagem",
                    "Use uma imagem JPG, PNG ou WebP de até 2 MB.");
        }

        if (!ModelState.IsValid) return await RetornarConferenciaAsync(model, entrada);

        var arquivosCriados = new List<string>();
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var idsProdutos = model.Itens.Where(i => i.ProdutoId.HasValue)
                .Select(i => i.ProdutoId!.Value).Distinct().ToList();
            var produtos = await _context.Produto.Include(p => p.Skus)
                .Where(p => idsProdutos.Contains(p.Id) && p.Ativo)
                .ToDictionaryAsync(p => p.Id);
            if (produtos.Count != idsProdutos.Count)
                throw new InvalidOperationException("Um produto selecionado não está mais disponível.");

            var produtosNovos = new Dictionary<string, Produto>(StringComparer.OrdinalIgnoreCase);
            var vinculosExistentes = await _context.VinculoProdutoFornecedor
                .Where(v => v.CnpjFornecedor == entrada.CnpjFornecedor)
                .Select(v => v.CodigoFornecedor).ToListAsync();
            var codigosVinculados = vinculosExistentes.ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var itemModel in model.Itens.OrderBy(i => i.NumeroItem))
            {
                var item = entrada.Itens.Single(i => i.Id == itemModel.ItemId);
                Produto produto;
                if (itemModel.ProdutoId.HasValue)
                {
                    produto = produtos[itemModel.ProdutoId.Value];
                }
                else
                {
                    var chaveProduto = string.Join('|', itemModel.NomeModelo!.Trim(), itemModel.Marca!.Trim(),
                        itemModel.Categoria!.Trim(), itemModel.Cor!.Trim()).ToUpperInvariant();
                    if (!produtosNovos.TryGetValue(chaveProduto, out produto!))
                    {
                        produto = new Produto
                        {
                            Id = Guid.NewGuid(), Nome = itemModel.NomeModelo.Trim(), Marca = itemModel.Marca.Trim(),
                            Categoria = itemModel.Categoria.Trim(), Cor = itemModel.Cor.Trim(), Ativo = true
                        };
                        _context.Produto.Add(produto);
                        produtosNovos[chaveProduto] = produto;
                    }
                }

                if (itemModel.Imagem is not null && string.IsNullOrWhiteSpace(produto.ImagemUrl))
                {
                    var caminho = await SalvarImagemAsync(itemModel.Imagem, produto.Id);
                    produto.ImagemUrl = caminho.Url;
                    arquivosCriados.Add(caminho.Fisico);
                }

                var sku = produto.Skus.FirstOrDefault(s =>
                    string.Equals(s.Numeracao, itemModel.Numeracao, StringComparison.OrdinalIgnoreCase));
                if (sku is null)
                {
                    sku = new Sku
                    {
                        Id = Guid.NewGuid(), ProdutoId = produto.Id,
                        Numeracao = itemModel.Numeracao, SaldoAtual = 0, Ativo = true
                    };
                    produto.Skus.Add(sku);
                }
                else if (!sku.Ativo)
                {
                    sku.Ativo = true;
                }

                sku.SaldoAtual += item.Quantidade;
                _context.Movimentacao.Add(new Movimentacao
                {
                    Id = Guid.NewGuid(), SkuId = sku.Id, Tipo = TipoMovimentacao.ENTRADA,
                    Quantidade = item.Quantidade, UsuarioId = usuarioId.Value, CriadoEm = DateTime.UtcNow,
                    Motivo = $"Entrada NF-e {entrada.Numero}"
                });
                item.ProdutoId = produto.Id;
                item.SkuId = sku.Id;

                if (codigosVinculados.Add(item.CodigoFornecedor))
                {
                    _context.VinculoProdutoFornecedor.Add(new VinculoProdutoFornecedor
                    {
                        Id = Guid.NewGuid(), CnpjFornecedor = entrada.CnpjFornecedor,
                        CodigoFornecedor = item.CodigoFornecedor, SkuId = sku.Id
                    });
                }
            }

            entrada.Status = StatusEntradaFiscal.CONFIRMADA;
            entrada.ConfirmadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["MensagemSucesso"] = $"NF-e {entrada.Numero} confirmada. Estoque atualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync();
            foreach (var arquivo in arquivosCriados.Where(System.IO.File.Exists)) System.IO.File.Delete(arquivo);
            ModelState.AddModelError(string.Empty,
                exception is InvalidOperationException ? exception.Message : "Não foi possível confirmar a entrada. Nenhum saldo foi alterado.");
            return await RetornarConferenciaAsync(model, entrada);
        }
    }

    private async Task<ConferirEntradaFiscalViewModel?> MontarConferenciaAsync(Guid id)
    {
        var entrada = await _context.EntradaFiscal.AsNoTracking().Include(e => e.Itens)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (entrada is null) return null;

        var vinculos = await _context.VinculoProdutoFornecedor.AsNoTracking()
            .Include(v => v.Sku).ThenInclude(s => s!.Produto)
            .Where(v => v.CnpjFornecedor == entrada.CnpjFornecedor)
            .ToDictionaryAsync(v => v.CodigoFornecedor, StringComparer.OrdinalIgnoreCase);
        var model = new ConferirEntradaFiscalViewModel
        {
            EntradaId = entrada.Id, Numero = entrada.Numero, ChaveAcesso = entrada.ChaveAcesso,
            FornecedorNome = entrada.FornecedorNome, CnpjFornecedor = entrada.CnpjFornecedor,
            Status = entrada.Status,
            Itens = entrada.Itens.OrderBy(i => i.NumeroItem).Select(i =>
            {
                vinculos.TryGetValue(i.CodigoFornecedor, out var vinculo);
                var sugestao = SepararDescricao(i.Descricao);
                return new ItemConferenciaEntradaFiscalViewModel
                {
                    ItemId = i.Id, NumeroItem = i.NumeroItem, CodigoFornecedor = i.CodigoFornecedor,
                    Descricao = i.Descricao, Quantidade = i.Quantidade, ValorUnitario = i.ValorUnitario,
                    ProdutoId = vinculo?.Sku?.ProdutoId, Numeracao = vinculo?.Sku?.Numeracao ?? sugestao.Numeracao,
                    NomeModelo = vinculo is null ? sugestao.Modelo : null,
                    Categoria = vinculo is null ? "Calçado" : null,
                    ImagemAtualUrl = vinculo?.Sku?.Produto?.ImagemUrl,
                    AssociadoAutomaticamente = vinculo is not null
                };
            }).ToList()
        };
        await PreencherProdutosAsync(model);
        return model;
    }

    private async Task<IActionResult> RetornarConferenciaAsync(
        ConferirEntradaFiscalViewModel model, EntradaFiscal entrada)
    {
        model.Numero = entrada.Numero;
        model.ChaveAcesso = entrada.ChaveAcesso;
        model.FornecedorNome = entrada.FornecedorNome;
        model.CnpjFornecedor = entrada.CnpjFornecedor;
        model.Status = entrada.Status;
        await PreencherProdutosAsync(model);
        return View("Conferir", model);
    }

    private async Task PreencherProdutosAsync(ConferirEntradaFiscalViewModel model)
    {
        model.Produtos = await _context.Produto.AsNoTracking().Where(p => p.Ativo)
            .OrderBy(p => p.Nome).ThenBy(p => p.Cor)
            .Select(p => new SelectListItem($"{p.Nome} · {p.Marca} · {p.Cor}", p.Id.ToString()))
            .ToListAsync();
    }

    private Guid? UsuarioId() => Guid.TryParse(
        User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private static async Task<DadosNfe> LerNfeAsync(IFormFile arquivo)
    {
        var settings = new XmlReaderSettings
        {
            Async = true, DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null,
            MaxCharactersInDocument = TamanhoMaximoXml
        };
        await using var stream = arquivo.OpenReadStream();
        using var reader = XmlReader.Create(stream, settings);
        var document = await XDocument.LoadAsync(reader, LoadOptions.None, CancellationToken.None);
        XElement? Primeiro(XContainer container, string nome) =>
            container.Descendants().FirstOrDefault(e => e.Name.LocalName == nome);
        string Valor(XContainer container, string nome) => Primeiro(container, nome)?.Value.Trim() ?? string.Empty;

        var infNfe = Primeiro(document, "infNFe") ?? throw new InvalidOperationException("O XML não contém uma NF-e.");
        var chave = Valor(document, "chNFe");
        if (chave.Length == 0) chave = (infNfe.Attribute("Id")?.Value ?? string.Empty).Replace("NFe", "", StringComparison.OrdinalIgnoreCase);
        if (chave.Length != 44 || chave.Any(c => !char.IsDigit(c)))
            throw new InvalidOperationException("A chave de acesso da NF-e é inválida.");

        var emit = Primeiro(infNfe, "emit") ?? throw new InvalidOperationException("O emitente não foi encontrado no XML.");
        var itens = new List<DadosItemNfe>();
        foreach (var det in infNfe.Elements().Where(e => e.Name.LocalName == "det"))
        {
            var prod = det.Elements().FirstOrDefault(e => e.Name.LocalName == "prod");
            if (prod is null) continue;
            if (!decimal.TryParse(Valor(prod, "qCom"), NumberStyles.Number, CultureInfo.InvariantCulture, out var quantidade) ||
                quantidade <= 0 || quantidade != decimal.Truncate(quantidade) || quantidade > int.MaxValue)
                throw new InvalidOperationException("Todos os itens devem possuir quantidade inteira positiva.");
            decimal.TryParse(Valor(prod, "vUnCom"), NumberStyles.Number, CultureInfo.InvariantCulture, out var valor);
            var codigo = Valor(prod, "cProd");
            var descricao = Valor(prod, "xProd");
            if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(descricao))
                throw new InvalidOperationException("Há item sem código ou descrição no XML.");
            var gtin = Valor(prod, "cEAN");
            itens.Add(new DadosItemNfe(
                int.TryParse(det.Attribute("nItem")?.Value, out var numeroItem) ? numeroItem : itens.Count + 1,
                codigo, gtin is "" or "SEM GTIN" ? null : gtin, descricao, (int)quantidade, valor));
        }
        if (itens.Count == 0) throw new InvalidOperationException("A NF-e não possui itens de produto.");
        return new DadosNfe(chave, Valor(infNfe, "nNF"), Valor(emit, "CNPJ"), Valor(emit, "xNome"), itens);
    }

    private static (string Modelo, string Numeracao) SepararDescricao(string descricao)
    {
        var partes = descricao.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var ultima = partes.LastOrDefault() ?? string.Empty;
        var numero = ultima.Length is >= 2 and <= 3 && ultima.All(char.IsDigit) ? ultima : string.Empty;
        return (numero.Length == 0 ? descricao : string.Join(' ', partes[..^1]), numero);
    }

    private static async Task<bool> ImagemValidaAsync(IFormFile imagem)
    {
        if (imagem.Length is <= 0 or > TamanhoMaximoImagem) return false;
        var header = new byte[12];
        await using var stream = imagem.OpenReadStream();
        var lidos = await stream.ReadAsync(header);
        var jpg = lidos >= 3 && header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff;
        var png = lidos >= 8 && header[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var webp = lidos >= 12 && System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF" &&
            System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP";
        return jpg || png || webp;
    }

    private async Task<(string Url, string Fisico)> SalvarImagemAsync(IFormFile imagem, Guid produtoId)
    {
        await using var leitura = imagem.OpenReadStream();
        var header = new byte[12];
        var lidos = await leitura.ReadAsync(header);
        leitura.Position = 0;
        var extensao = lidos >= 8 && header[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })
            ? ".png"
            : lidos >= 12 && System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP" ? ".webp" : ".jpg";
        var diretorio = Path.Combine(_environment.WebRootPath, "uploads", "produtos");
        Directory.CreateDirectory(diretorio);
        var nome = $"{produtoId:N}-{Guid.NewGuid():N}{extensao}";
        var fisico = Path.Combine(diretorio, nome);
        await using var destino = System.IO.File.Create(fisico);
        await leitura.CopyToAsync(destino);
        return ($"/uploads/produtos/{nome}", fisico);
    }

    private sealed record DadosNfe(string ChaveAcesso, string Numero, string CnpjFornecedor,
        string FornecedorNome, IReadOnlyList<DadosItemNfe> Itens);
    private sealed record DadosItemNfe(int NumeroItem, string CodigoFornecedor, string? Gtin,
        string Descricao, int Quantidade, decimal ValorUnitario);
}
