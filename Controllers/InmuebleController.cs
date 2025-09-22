using InmobiliariaAdo.Data;
using InmobiliariaAdo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace InmobiliariaAdo.Controllers
{
    [Authorize] // 🔒 restringido a usuarios logueados
    public class InmueblesController : Controller
    {
        private readonly InmuebleRepositorio _repo;
        private readonly PropietarioRepositorio _repoProp;
        private readonly TipoInmuebleRepositorio _repoTipo; // 👈 agregado

        public InmueblesController(InmuebleRepositorio repo, PropietarioRepositorio repoProp, TipoInmuebleRepositorio repoTipo) // 👈 agregado
        {
            _repo = repo;
            _repoProp = repoProp;
            _repoTipo = repoTipo; // 👈 agregado
        }

        // GET /Inmuebles
        public async Task<IActionResult> Index()
        {
            var lista = await _repo.ListarAsync();
            return View(lista);
        }

        // GET /Inmuebles/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var x = await _repo.ObtenerPorIdAsync(id);
            if (x == null) return NotFound();
            return View(x);
        }

        // GET /Inmuebles/Create
        public async Task<IActionResult> Create()
        {
            await CargarCombosAsync();
            return View();
        }

        // POST /Inmuebles/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inmueble x)
        {
            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(x.PropietarioId, x.TipoId);
                return View(x);
            }

            var id = await _repo.CrearAsync(x);
            TempData["Msg"] = $"Inmueble creado (Id {id}).";
            return RedirectToAction(nameof(Index));
        }

        // GET /Inmuebles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var x = await _repo.ObtenerPorIdAsync(id);
            if (x == null) return NotFound();
            await CargarCombosAsync(x.PropietarioId, x.TipoId);
            return View(x);
        }

        // POST /Inmuebles/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Inmueble x)
        {
            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(x.PropietarioId, x.TipoId);
                return View(x);
            }

            var ok = await _repo.ActualizarAsync(x);
            if (!ok) return NotFound();

            TempData["Msg"] = $"Inmueble Id {x.Id} actualizado.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Inmuebles/Delete/5
        [Authorize(Policy = "EsAdmin")] // 🔒 solo admins eliminan
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _repo.ObtenerPorIdAsync(id);
            if (x == null) return NotFound();
            return View(x);
        }

        // POST /Inmuebles/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Policy = "EsAdmin")] // 🔒 solo admins eliminan
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _repo.EliminarAsync(id);
                TempData["SuccessMessage"] = "✅ Inmueble eliminado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var mensajeError = ex.InnerException?.Message ?? ex.Message;

                if (mensajeError.Contains("1451"))
                {
                    TempData["ErrorMessage"] = "❌ No puedes eliminar este Inmueble porque tiene contratos o pagos asociados.";
                }
                else
                {
                    TempData["ErrorMessage"] = "⚠️ Ocurrió un error al intentar eliminar el Inmueble.";
                }

                return RedirectToAction(nameof(Index));
            }
        }

        // ================= helpers =================
        private async Task CargarCombosAsync(int? propietarioSel = null, int? tipoSel = null)
        {
            var props = await _repoProp.ListarAsync();
            ViewBag.Propietarios = props
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Apellido}, {p.Nombre}",
                    Selected = (propietarioSel == p.Id)
                })
                .OrderBy(s => s.Text)
                .ToList();

            var tipos = await _repoTipo.ListarAsync();
            ViewBag.Tipos = tipos
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Nombre,
                    Selected = (tipoSel == t.Id)
                })
                .OrderBy(s => s.Text)
                .ToList();
        }

        // ================= inmuebles libres =================
        [HttpGet]
        public IActionResult Libres()
        {
            return View(); // muestra formulario con 2 fechas
        }

        [HttpPost]
        public async Task<IActionResult> Libres(DateTime inicio, DateTime fin)
        {
            var lista = await _repo.ListarLibresEntreFechasAsync(inicio, fin);
            ViewBag.FechaInicio = inicio.ToShortDateString();
            ViewBag.FechaFin = fin.ToShortDateString();
            return View("LibresResultado", lista);
        }
    }
}

