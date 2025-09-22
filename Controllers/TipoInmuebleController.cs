using Microsoft.AspNetCore.Mvc;
using InmobiliariaAdo.Data;
using InmobiliariaAdo.Models;

namespace InmobiliariaAdo.Controllers
{
    public class TipoInmuebleController : Controller
    {
        private readonly TipoInmuebleRepositorio _repo;

        public TipoInmuebleController(IConfiguration config)
        {
            _repo = new TipoInmuebleRepositorio(config);
        }

        // GET: TiposInmueble
        public async Task<IActionResult> Index()
        {
            var lista = await _repo.ListarAsync();
            return View(lista);
        }

        // GET: TiposInmueble/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var tipo = await _repo.ObtenerPorIdAsync(id);
            if (tipo == null) return NotFound();
            return View(tipo);
        }

        // GET: TiposInmueble/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TiposInmueble/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TipoInmueble tipo)
        {
            if (ModelState.IsValid)
            {
                await _repo.CrearAsync(tipo);
                return RedirectToAction(nameof(Index));
            }
            return View(tipo);
        }

        // GET: TiposInmueble/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var tipo = await _repo.ObtenerPorIdAsync(id);
            if (tipo == null) return NotFound();
            return View(tipo);
        }

        // POST: TiposInmueble/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TipoInmueble tipo)
        {
            if (ModelState.IsValid)
            {
                await _repo.ActualizarAsync(tipo);
                return RedirectToAction(nameof(Index));
            }
            return View(tipo);
        }

        // GET: TiposInmueble/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var tipo = await _repo.ObtenerPorIdAsync(id);
            if (tipo == null) return NotFound();
            return View(tipo);
        }

        //POST: TiposInmueble/Delete/5
       

[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    try
    {
        await _repo.EliminarAsync(id);
        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        if (ex.InnerException != null && ex.InnerException.Message.Contains("1451"))
        {
            // Error de clave foránea (no se puede eliminar porque está en uso)
            TempData["ErrorMessage"] = "❌ No puedes eliminar este Tipo de Inmueble porque está siendo usado por un Inmueble.";
        }
        else
        {
            TempData["ErrorMessage"] = "⚠️ Ocurrió un error al intentar eliminar el Tipo de Inmueble.";
        }

        return RedirectToAction(nameof(Index));
    }
}


    }
}
