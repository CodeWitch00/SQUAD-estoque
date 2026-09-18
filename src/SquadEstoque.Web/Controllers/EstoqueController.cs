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
                Cor = produto.Cor
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
            viewModel.ProdutoSelecionadoId = produtoId;
            produtoSelecionado.Skus = await _context.Sku
                .AsNoTracking()
                .Where(sku => sku.ProdutoId == produtoSelecionado.Id && sku.Ativo)
                .OrderBy(sku => sku.Numeracao)
                .Select(sku => new SkuConsultaResultadoViewModel
                {
                    Id = sku.Id,
                    Numeracao = sku.Numeracao,
                    SaldoAtual = sku.SaldoAtual
                })
                .ToListAsync();
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Vender(Guid skuId)
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

        try
        {
            var skuDisponivel = await _context.Sku
                .AsNoTracking()
                .AnyAsync(sku => sku.Id == skuId && sku.Ativo && sku.Produto != null && sku.Produto.Ativo);

            if (!skuDisponivel)
            {
                return BadRequest(new { mensagem = "O SKU selecionado não está disponível para venda." });
            }

            var resultado = await _context.RegistrarVendaRapidaAsync(skuId, usuarioId.Value);
            if (resultado.Erro is not null)
            {
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

    private Guid? GetAuthenticatedUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claimValue, out var id) ? id : null;
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
