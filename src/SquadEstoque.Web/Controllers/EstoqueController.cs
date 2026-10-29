using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;

namespace SquadEstoque.Web.Controllers;

[Authorize(Roles = "VENDEDOR")]
public sealed class EstoqueController : Controller
{
    private const int TamanhoMinimoTermo = 2;
    private const int TamanhoMaximoTermo = 100;
    private readonly EstoqueContext _context;

    public EstoqueController(EstoqueContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Consulta(string? termo, Guid? produtoId, Guid? atendimentoId)
    {
        var usuarioId = GetAuthenticatedUserId();
        if (!usuarioId.HasValue)
        {
            return Challenge();
        }

        var viewModel = new ConsultaEstoqueViewModel
        {
            Termo = termo?.Trim() ?? string.Empty
        };
        await CarregarAtendimentosPendentesAsync(viewModel, usuarioId.Value);

        if (atendimentoId.HasValue)
        {
            var atendimento = await _context.Atendimento
                .AsNoTracking()
                .Include(a => a.Produto)
                .ThenInclude(p => p!.Skus)
                .FirstOrDefaultAsync(a => a.Id == atendimentoId.Value &&
                    a.UsuarioId == usuarioId.Value && a.Status == StatusAtendimento.PENDENTE);

            if (atendimento?.Produto is null)
            {
                viewModel.MensagemEstado = "Atendimento pendente não encontrado.";
                return View(viewModel);
            }

            var produto = new ProdutoConsultaResultadoViewModel
            {
                Id = atendimento.Produto.Id,
                Nome = atendimento.Produto.Nome,
                Marca = atendimento.Produto.Marca,
                Categoria = atendimento.Produto.Categoria,
                Cor = atendimento.Produto.Cor,
                ImagemUrl = atendimento.Produto.ImagemUrl,
                Skus = atendimento.Produto.Skus
                    .Where(sku => sku.Ativo)
                    .OrderBy(sku => sku.Numeracao)
                    .Select(sku => new SkuConsultaResultadoViewModel
                    {
                        Id = sku.Id,
                        Numeracao = sku.Numeracao,
                        SaldoAtual = sku.SaldoAtual,
                        UltimaAtualizacaoSaldoEm = sku.UltimaAtualizacaoSaldoEm
                    })
                    .ToList()
            };
            viewModel.AtendimentoId = atendimento.Id;
            viewModel.ProdutoSelecionadoId = produto.Id;
            viewModel.SkuSelecionadoId = atendimento.SkuId;
            viewModel.ModeloTotalmenteIndisponivel = produto.Skus.Count == 0 ||
                produto.Skus.All(sku => sku.SaldoAtual == 0);
            viewModel.Resultados.Add(produto);
            return View(viewModel);
        }

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

        var termoNormalizado = Normalizar(viewModel.Termo);
        if (termoNormalizado.Length == 0)
        {
            viewModel.MensagemEstado = "Produto não encontrado.";
            return View(viewModel);
        }

        var primeiroCaractere = termoNormalizado[0].ToString();
        var ultimoCaractere = termoNormalizado[^1].ToString();
        var padraoPrimeiroCaractere = $"%{EscapeLikePattern(primeiroCaractere)}%";
        var padraoUltimoCaractere = $"%{EscapeLikePattern(ultimoCaractere)}%";

        var produtos = await _context.Produto
            .AsNoTracking()
            .Where(produto => produto.Ativo &&
                (EF.Functions.Like(produto.Nome, padraoPrimeiroCaractere, "\\") ||
                 EF.Functions.Like(produto.Marca, padraoPrimeiroCaractere, "\\") ||
                 EF.Functions.Like(produto.Categoria, padraoPrimeiroCaractere, "\\") ||
                 EF.Functions.Like(produto.Cor, padraoPrimeiroCaractere, "\\")) &&
                (EF.Functions.Like(produto.Nome, padraoUltimoCaractere, "\\") ||
                 EF.Functions.Like(produto.Marca, padraoUltimoCaractere, "\\") ||
                 EF.Functions.Like(produto.Categoria, padraoUltimoCaractere, "\\") ||
                 EF.Functions.Like(produto.Cor, padraoUltimoCaractere, "\\")))
            .Select(produto => new ProdutoConsultaResultadoViewModel
            {
                Id = produto.Id,
                Nome = produto.Nome,
                Marca = produto.Marca,
                Categoria = produto.Categoria,
                Cor = produto.Cor,
                ImagemUrl = produto.ImagemUrl
            })
            .ToListAsync();

        viewModel.Resultados = produtos
            .Where(produto => CorrespondeAoTermo(produto, viewModel.Termo))
            .OrderBy(produto => produto.Nome)
            .ThenBy(produto => produto.Marca)
            .ThenBy(produto => produto.Cor)
            .ToList();

        if (viewModel.Resultados.Count == 0)
        {
            viewModel.MensagemEstado = "Produto não encontrado.";
            return View(viewModel);
        }

        var produtoSelecionado = produtoId.HasValue
            ? viewModel.Resultados.FirstOrDefault(produto => produto.Id == produtoId.Value)
            : null;

        if (produtoSelecionado is not null)
        {
            viewModel.ProdutoSelecionadoId = produtoSelecionado.Id;
            produtoSelecionado.Skus = await _context.Sku
                .AsNoTracking()
                .Where(sku => sku.ProdutoId == produtoSelecionado.Id && sku.Ativo)
                .OrderBy(sku => sku.Numeracao)
                .Select(sku => new SkuConsultaResultadoViewModel
                {
                    Id = sku.Id,
                    Numeracao = sku.Numeracao,
                    SaldoAtual = sku.SaldoAtual,
                    UltimaAtualizacaoSaldoEm = sku.UltimaAtualizacaoSaldoEm
                })
                .ToListAsync();
            viewModel.ModeloTotalmenteIndisponivel = produtoSelecionado.Skus.Count == 0 ||
                produtoSelecionado.Skus.All(sku => sku.SaldoAtual == 0);

        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelecionarSku(Guid? atendimentoId, Guid skuId)
    {
        var usuarioId = GetAuthenticatedUserId();
        if (!usuarioId.HasValue)
        {
            return Challenge();
        }

        var atendimento = atendimentoId.HasValue
            ? await _context.Atendimento.FirstOrDefaultAsync(a =>
                a.Id == atendimentoId.Value && a.UsuarioId == usuarioId.Value &&
                a.Status == StatusAtendimento.PENDENTE)
            : null;
        if (atendimentoId.HasValue && atendimento is null)
        {
            return BadRequest(new { mensagem = "O atendimento não está pendente ou não pertence ao vendedor." });
        }

        var sku = await _context.Sku.AsNoTracking().FirstOrDefaultAsync(s =>
            s.Id == skuId && (!atendimentoId.HasValue || s.ProdutoId == atendimento!.ProdutoId) && s.Ativo &&
            s.Produto != null && s.Produto.Ativo);
        if (sku is null)
        {
            return BadRequest(new { mensagem = "A numeração selecionada não pertence ao atendimento." });
        }

        if (sku.SaldoAtual <= 0)
        {
            return Ok(new
            {
                atendimentoId = atendimento?.Id,
                skuId = sku.Id,
                numeracao = sku.Numeracao,
                saldoConsultado = sku.SaldoAtual,
                persistido = false
            });
        }

        if (atendimento is null)
        {
            atendimento = new Atendimento
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId.Value,
                ProdutoId = sku.ProdutoId,
                Status = StatusAtendimento.PENDENTE,
                CriadoEm = DateTime.UtcNow
            };
            _context.Atendimento.Add(atendimento);
        }
        atendimento.SkuId = sku.Id;
        atendimento.SaldoConsultado = sku.SaldoAtual;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            atendimentoId = atendimento.Id,
            skuId = sku.Id,
            numeracao = sku.Numeracao,
            saldoConsultado = sku.SaldoAtual,
            persistido = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarDesistiu(Guid atendimentoId)
    {
        var usuarioId = GetAuthenticatedUserId();
        if (!usuarioId.HasValue)
        {
            return Challenge();
        }

        var atendimento = await _context.Atendimento.FirstOrDefaultAsync(a =>
            a.Id == atendimentoId && a.UsuarioId == usuarioId.Value &&
            a.Status == StatusAtendimento.PENDENTE);
        if (atendimento is null)
        {
            return BadRequest(new { mensagem = "O atendimento não está pendente ou não pertence ao vendedor." });
        }

        atendimento.Status = StatusAtendimento.CONCLUIDO;
        atendimento.Resultado = ResultadoAtendimento.DESISTIU;
        atendimento.ConcluidoEm = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            resultado = "desistiu",
            atendimentoEncerrado = true,
            mensagem = "Atendimento encerrado.",
            novaConsultaUrl = Url.Action(nameof(Consulta), "Estoque")
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Vender(Guid skuId, Guid produtoId, Guid atendimentoId)
    {
        var usuarioId = GetAuthenticatedUserId();
        if (!usuarioId.HasValue)
        {
            return Challenge();
        }

        if (skuId == Guid.Empty)
        {
            return BadRequest(new { mensagem = "Informe um SKU válido para registrar a venda." });
        }

        if (produtoId == Guid.Empty)
        {
            return BadRequest(new { mensagem = "Informe um produto válido para registrar a venda." });
        }

        try
        {
            var skuDisponivel = await _context.Sku
                .AsNoTracking()
                .AnyAsync(sku => sku.Id == skuId && sku.ProdutoId == produtoId &&
                    sku.Ativo && sku.Produto != null && sku.Produto.Ativo);

            if (!skuDisponivel)
            {
                return BadRequest(new { mensagem = "O SKU selecionado não está disponível para venda." });
            }

            var atendimento = await ObterOuCriarAtendimentoPendenteAsync(
                atendimentoId, usuarioId.Value, produtoId);
            if (atendimento is null)
            {
                return BadRequest(new { mensagem = "O atendimento não está pendente ou não pertence ao vendedor." });
            }

            var resultado = await _context.RegistrarVendaRapidaAsync(skuId, usuarioId.Value, atendimento.Id);
            if (resultado.Erro is not null)
            {
                if (resultado.Conflito)
                    return Conflict(new { mensagem = resultado.Erro });
                return BadRequest(new { mensagem = resultado.Erro });
            }

            return Ok(new
            {
                mensagem = "Venda registrada com sucesso.",
                skuId,
                saldoAtual = resultado.Sku!.SaldoAtual
            });
        }
        catch
        {
            return StatusCode(500, new
            {
                mensagem = "Não foi possível registrar a venda. Tente novamente."
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarNaoTinha(Guid skuId, Guid produtoId, Guid? atendimentoId)
    {
        var usuarioId = GetAuthenticatedUserId();
        if (!usuarioId.HasValue)
        {
            return Challenge();
        }

        if (skuId == Guid.Empty)
        {
            return BadRequest(new { mensagem = "Selecione uma numeração para registrar a ruptura." });
        }

        if (produtoId == Guid.Empty || !ModelState.IsValid)
        {
            return BadRequest(new { mensagem = "A numeração selecionada não está disponível para registro." });
        }

        var skuDisponivel = await _context.Sku
            .AsNoTracking()
            .FirstOrDefaultAsync(sku => sku.Id == skuId && sku.ProdutoId == produtoId &&
                sku.Ativo && sku.Produto != null && sku.Produto.Ativo);

        if (skuDisponivel is null)
        {
            return BadRequest(new { mensagem = "A numeração selecionada não está disponível para registro." });
        }

        var atendimento = await ObterOuCriarAtendimentoPendenteAsync(
            atendimentoId, usuarioId.Value, produtoId);
        if (atendimento is null)
        {
            return BadRequest(new { mensagem = "O atendimento não está pendente ou não pertence ao vendedor." });
        }

        var ruptura = new Ruptura
        {
            Id = Guid.NewGuid(),
            SkuId = skuId,
            UsuarioId = usuarioId.Value,
            CriadoEm = DateTime.UtcNow,
            AtendimentoId = atendimento.Id
        };

        atendimento.SkuId = skuId;
        atendimento.SaldoConsultado = skuDisponivel.SaldoAtual;
        atendimento.Status = StatusAtendimento.CONCLUIDO;
        atendimento.Resultado = ResultadoAtendimento.NAO_TINHA;
        atendimento.ConcluidoEm = DateTime.UtcNow;
        _context.Ruptura.Add(ruptura);
        await _context.SaveChangesAsync();

        return Ok(new { mensagem = "Ruptura registrada com sucesso.", skuId });
    }

    private Guid? GetAuthenticatedUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claimValue, out var id) ? id : null;
    }

    private async Task CarregarAtendimentosPendentesAsync(
        ConsultaEstoqueViewModel viewModel, Guid usuarioId)
    {
        viewModel.AtendimentosPendentes = await _context.Atendimento
            .AsNoTracking()
            .Where(a => a.UsuarioId == usuarioId && a.Status == StatusAtendimento.PENDENTE)
            .OrderByDescending(a => a.CriadoEm)
            .Select(a => new AtendimentoPendenteViewModel
            {
                Id = a.Id,
                ProdutoNome = a.Produto!.Nome,
                ProdutoDetalhes = a.Produto.Marca + " · " + a.Produto.Cor,
                ImagemUrl = a.Produto.ImagemUrl,
                Numeracao = a.Sku != null ? a.Sku.Numeracao : null,
                CriadoEm = a.CriadoEm
            })
            .ToListAsync();
    }

    private async Task<Atendimento?> ObterOuCriarAtendimentoPendenteAsync(
        Guid? atendimentoId, Guid usuarioId, Guid produtoId)
    {
        if (atendimentoId.HasValue && atendimentoId.Value != Guid.Empty)
        {
            return await _context.Atendimento.FirstOrDefaultAsync(a =>
                a.Id == atendimentoId.Value && a.UsuarioId == usuarioId &&
                a.ProdutoId == produtoId && a.Status == StatusAtendimento.PENDENTE);
        }

        var produtoAtivo = await _context.Produto.AsNoTracking()
            .AnyAsync(p => p.Id == produtoId && p.Ativo);
        if (!produtoAtivo) return null;

        var atendimento = new Atendimento
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            ProdutoId = produtoId,
            Status = StatusAtendimento.PENDENTE,
            CriadoEm = DateTime.UtcNow
        };
        _context.Atendimento.Add(atendimento);
        await _context.SaveChangesAsync();
        return atendimento;
    }

    private static bool CorrespondeAoTermo(ProdutoConsultaResultadoViewModel produto, string termo)
    {
        var palavras = Normalizar(termo)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var campos = Normalizar(string.Join(' ', produto.Nome, produto.Marca, produto.Categoria, produto.Cor));

        return palavras.All(palavra => campos.Contains(palavra, StringComparison.Ordinal) ||
            campos.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Any(campo => palavra.Length >= 4 && DistanciaDeLevenshtein(palavra, campo) <= 1));
    }

    private static string Normalizar(string valor)
    {
        var decomposicao = valor.Normalize(NormalizationForm.FormD);
        var semAcentos = new StringBuilder(decomposicao.Length);

        foreach (var caractere in decomposicao)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
            {
                semAcentos.Append(char.ToLowerInvariant(caractere));
            }
        }

        return semAcentos.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }

    private static int DistanciaDeLevenshtein(string esquerda, string direita)
    {
        var linhaAnterior = Enumerable.Range(0, direita.Length + 1).ToArray();

        for (var i = 1; i <= esquerda.Length; i++)
        {
            var linhaAtual = new int[direita.Length + 1];
            linhaAtual[0] = i;

            for (var j = 1; j <= direita.Length; j++)
            {
                linhaAtual[j] = Math.Min(
                    Math.Min(linhaAtual[j - 1] + 1, linhaAnterior[j] + 1),
                    linhaAnterior[j - 1] + (esquerda[i - 1] == direita[j - 1] ? 0 : 1));
            }

            linhaAnterior = linhaAtual;
        }

        return linhaAnterior[direita.Length];
    }

}
