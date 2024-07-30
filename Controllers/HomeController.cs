using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using apinet.Models;
using Microsoft.AspNetCore.Authorization;
namespace apinet.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
[Authorize]
    public IActionResult Index()
    {
              var claims =User.Claims;
            string Rol = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value;
            ViewBag.Rol=Rol;
        return View();
    }

    public IActionResult Privacy()
    {
              var claims =User.Claims;
            string Rol = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role).Value;
            ViewBag.Rol=Rol;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult Restringido()
{
    ViewBag.ErrorMessage = "Solo el administrador puede realizar esta acción.";
    return View(); // O devuelve un mensaje de texto directamente
}
}
