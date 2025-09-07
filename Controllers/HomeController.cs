using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using InmobiliariaAdo.Models;
using InmobiliariaAdo.Data;

namespace InmobiliariaAdo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly InmuebleRepositorio _repoInmuebles;

        public HomeController(ILogger<HomeController> logger, InmuebleRepositorio repoInmuebles)
        {
            _logger = logger;
            _repoInmuebles = repoInmuebles;
        }

        // Página principal
        public async Task<IActionResult> Index()
        {
            // lista de inmuebles disponibles a la fecha de hoy
            var disponibles = await _repoInmuebles.ListarDisponiblesHoyAsync();

            // los paso al modelo de la vista
            ViewBag.DisponiblesHoy = disponibles.Count;

            return View(disponibles); 
            // la vista Index.cshtml recibirá una lista de Inmueble como modelo
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
}
