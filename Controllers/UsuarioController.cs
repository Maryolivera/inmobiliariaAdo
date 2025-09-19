using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InmobiliariaAdo.Data;
using InmobiliariaAdo.Models;

namespace InmobiliariaAdo.Controllers
{
    [Authorize(Policy = "EsAdmin")] // 🚨 solo administradores
    public class UsuariosController : Controller
    {
        private readonly UsuarioRepositorio _repo;

        public UsuariosController(UsuarioRepositorio repo)
        {
            _repo = repo;
        }

        // GET: /Usuarios
        public async Task<IActionResult> Index()
        {
            var lista = await _repo.ListarAsync();
            return View(lista);
        }

        // GET: /Usuarios/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: /Usuarios/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Usuario u, string clavePlano)
        {
            if (ModelState.IsValid)
            {
                await _repo.CrearAsync(u, clavePlano);
                return RedirectToAction(nameof(Index));
            }
            return View(u);
        }

        // GET: /Usuarios/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var u = await _repo.BuscarPorIdAsync(id);
            if (u == null) return NotFound();
            return View(u);
        }

        // POST: /Usuarios/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Usuario u)
        {
            if (ModelState.IsValid)
            {
                await _repo.ActualizarAsync(u);
                return RedirectToAction(nameof(Index));
            }
            return View(u);
        }

        // POST: /Usuarios/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _repo.EliminarAsync(id); // 👈 borrado lógico (Activo=0)
            return RedirectToAction(nameof(Index));
        }
    }
}
