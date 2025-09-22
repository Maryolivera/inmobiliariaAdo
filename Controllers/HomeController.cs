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
        private readonly UsuarioRepositorio _repoUsuarios;  

        // Inyectamos ambos repositorios
        public HomeController(ILogger<HomeController> logger, InmuebleRepositorio repoInmuebles, UsuarioRepositorio repoUsuarios)
        {
            _logger = logger;
            _repoInmuebles = repoInmuebles;
            _repoUsuarios = repoUsuarios;   
        }

        // Página principal
        public async Task<IActionResult> Index()
        {
            var disponibles = await _repoInmuebles.ListarDisponiblesHoyAsync();
            ViewBag.DisponiblesHoy = disponibles.Count;
            return View(disponibles);
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

        // Acción de prueba de hash
        [HttpGet]
        public IActionResult TestHash(string clave = "1234")
        {
            // usamos reflexión para llamar al método privado Hash
            var miMetodo = typeof(UsuarioRepositorio)
                .GetMethod("Hash", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var hash = miMetodo.Invoke(_repoUsuarios, new object[] { clave });  // 👈 usamos _repoUsuarios

            return Content($"Clave: {clave}\nHash: {hash}");
        }
    }
}
