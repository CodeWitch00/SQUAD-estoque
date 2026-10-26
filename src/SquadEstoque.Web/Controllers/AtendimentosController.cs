using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SquadEstoque.Web.Data;
using SquadEstoque.Web.Models;

namespace SquadEstoque.Web.Controllers;

[Authorize(Roles = "LOJISTA")]
public sealed class AtendimentosController : Controller
{
    private readonly EstoqueContext _context;

    public AtendimentosController(EstoqueContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var agora = DateTime.UtcNow;
        var registros = await _context.Atendimento
            .AsNoTracking()
            .Include(a => a.Usuario)
            .Include(a => a.Produto)
            .Include(a => a.Sku)
            .OrderByDescending(a => a.CriadoEm)
            .Take(200)
            .ToListAsync();

        var itens = registros.Select(a => new AtendimentoAuditoriaViewModel
        {
            Id = a.Id,
            Vendedor = a.Usuario?.Nome ?? "Vendedor não encontrado",
            Produto = a.Produto?.Nome ?? "Produto não encontrado",
            Numeracao = a.Sku?.Numeracao,
            SaldoConsultado = a.SaldoConsultado,
            Status = a.Status,
            Resultado = a.Resultado,
            CriadoEm = a.CriadoEm,
            ConcluidoEm = a.ConcluidoEm,
            Alerta = ObterAlerta(a, agora)
        }).ToList();

        return View(new AtendimentosIndexViewModel
        {
            TotalPendentes = itens.Count(a => a.Status == StatusAtendimento.PENDENTE),
            TotalConcluidos = itens.Count(a => a.Status == StatusAtendimento.CONCLUIDO),
            TotalAlertas = itens.Count(a => a.Alerta is not null),
            Atendimentos = itens
        });
    }

    private static string? ObterAlerta(Atendimento atendimento, DateTime agora)
    {
        if (atendimento.Status == StatusAtendimento.PENDENTE &&
            agora - atendimento.CriadoEm >= TimeSpan.FromMinutes(15))
        {
            return "Pendente há mais de 15 minutos";
        }

        if (atendimento.ConcluidoEm.HasValue &&
            atendimento.ConcluidoEm.Value - atendimento.CriadoEm <= TimeSpan.FromSeconds(5))
        {
            return "Encerrado em menos de 5 segundos";
        }

        if (atendimento.Resultado == ResultadoAtendimento.NAO_TINHA &&
            atendimento.SaldoConsultado > 0)
        {
            return "Não tinha informado com saldo positivo";
        }

        return null;
    }
}
