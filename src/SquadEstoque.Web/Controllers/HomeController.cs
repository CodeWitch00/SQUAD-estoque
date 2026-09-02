using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SquadEstoque.Web.Models;

namespace SquadEstoque.Web.Controllers;

public class HomeController : Controller
{
    [Authorize]
    public IActionResult Index()
    {
        if (User.IsInRole("VENDEDOR"))
        {
            return RedirectToAction("Consulta", "Estoque");
        }

        if (User.IsInRole("LOJISTA"))
        {
            return RedirectToAction("Index", "Produtos");
        }

        return RedirectToAction("AccessDenied", "Account");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
