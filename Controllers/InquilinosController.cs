using InmobiliariaAdo.Data;
using InmobiliariaAdo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace InmobiliariaAdo.Controllers
{
    [Authorize] // 🔒 restringido a usuarios logueados
    

    public class InquilinosController : Controller
    {
        private readonly InquilinoRepositorio _repo;
        public InquilinosController(InquilinoRepositorio repo) => _repo = repo;

        // GET /Inquilinos
        public async Task<IActionResult> Index()
        {
            var lista = await _repo.ListarAsync();
            return View(lista);
        }

        // GET /Inquilinos/Create
        public IActionResult Create() => View();

        // POST /Inquilinos/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inquilino x)
        {
            if (!ModelState.IsValid) return View(x);
            var id = await _repo.CrearAsync(x);
            TempData["Msg"] = $"Inquilino creado (Id {id}).";
            return RedirectToAction(nameof(Index));
        }

        // GET /Inquilinos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var x = await _repo.ObtenerPorIdAsync(id);
            if (x == null) return NotFound();
            return View(x);
        }

        // POST /Inquilinos/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Inquilino x)
        {
            if (!ModelState.IsValid) return View(x);
            var ok = await _repo.ActualizarAsync(x);
            if (!ok) return NotFound();
            TempData["Msg"] = $"Inquilino Id {x.Id} actualizado.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Inquilinos/Delete/5
        [Authorize(Policy = "EsAdmin")] 
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _repo.ObtenerPorIdAsync(id);
            if (x == null) return NotFound();
            return View(x);
        }

        // POST /Inquilinos/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Policy = "EsAdmin")] 

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ok = await _repo.EliminarAsync(id);
            if (!ok) return NotFound();
            TempData["Msg"] = $"Inquilino Id {id} eliminado.";
            return RedirectToAction(nameof(Index));
        }
        // GET: /Inquilinos/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var x = await _repo.ObtenerPorIdAsync(id);
            if (x == null) return NotFound();
            return View(x); // busca Views/Inquilinos/Details.cshtml
        }

    }
}