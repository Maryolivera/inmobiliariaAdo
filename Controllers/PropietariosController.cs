using InmobiliariaAdo.Data;
using InmobiliariaAdo.Models;
using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaAdo.Controllers
{
    public class PropietariosController : Controller
    {
        private readonly PropietarioRepositorio _repo;

        public PropietariosController(PropietarioRepositorio repo)
        {
            _repo = repo;
        }

        // GET: /Propietarios
        public async Task<IActionResult> Index()
        {
            var lista = await _repo.ListarAsync();
            return View(lista);
        }

        // GET: /Propietarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Propietarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Propietario p)
        {
            if (!ModelState.IsValid)
                return View(p);

            var nuevoId = await _repo.CrearAsync(p);
            TempData["Msg"] = $"Propietario creado (Id {nuevoId}).";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Propietarios/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _repo.ObtenerPorIdAsync(id);
            if (p == null) return NotFound();
            return View(p);
        }

        // POST: /Propietarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Propietario p)
        {
            if (id != p.Id) return BadRequest();
            if (!ModelState.IsValid) return View(p);

            var ok = await _repo.ActualizarAsync(p);
            if (!ok) return NotFound();
            TempData["Msg"] = $"Propietario Id {p.Id} actualizado.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Propietarios/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _repo.ObtenerPorIdAsync(id);
            if (p == null) return NotFound();
            return View(p); // vista de confirmación
        }

        // POST: /Propietarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ok = await _repo.EliminarAsync(id);
            if (!ok) return NotFound();
            TempData["Msg"] = $"Propietario Id {id} eliminado.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Propietarios/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var x = await _repo.ObtenerPorIdAsync(id);
            if (x == null) return NotFound();
            return View(x);
        }
    }
}
