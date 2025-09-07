using InmobiliariaAdo.Data;
using InmobiliariaAdo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InmobiliariaAdo.Controllers
{
    public class InmueblesController : Controller
    {
        private readonly InmuebleRepositorio _repo;
        private readonly PropietarioRepositorio _repoProp; // <-- agregado

        public InmueblesController(InmuebleRepositorio repo, PropietarioRepositorio repoProp)
        {
            _repo = repo;
            _repoProp = repoProp;
        }

        // GET /Inmuebles
        public async Task<IActionResult> Index()
        {
            // ListarAsync del repo debe venir con JOIN (direccion + PropietarioNombre)
            var lista = await _repo.ListarAsync();
            return View(lista);
        }

        // GET /Inmuebles/Create
        public async Task<IActionResult> Create()
        {
            await CargarPropietariosAsync();
            return View();
        }

        // POST /Inmuebles/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inmueble x)
        {
            if (!ModelState.IsValid)
            {
                await CargarPropietariosAsync();
                return View(x);
            }

            var id = await _repo.CrearAsync(x);
            TempData["Msg"] = $"Inmueble creado (Id {id}).";
            return RedirectToAction(nameof(Index));
        }

        // GET /Inmuebles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var x = await _repo.ObtenerPorIdAsync(id); // debe traer PropietarioNombre por JOIN
            if (x == null) return NotFound();
            await CargarPropietariosAsync(x.PropietarioId);
            return View(x);
        }

        // POST /Inmuebles/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Inmueble x)
        {
            if (!ModelState.IsValid)
            {
                await CargarPropietariosAsync(x.PropietarioId);
                return View(x);
            }

            var ok = await _repo.ActualizarAsync(x);
            if (!ok) return NotFound();

            TempData["Msg"] = $"Inmueble Id {x.Id} actualizado.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Inmuebles/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _repo.ObtenerPorIdAsync(id);
            if (x == null) return NotFound();
            return View(x);
        }

        // POST /Inmuebles/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ok = await _repo.EliminarAsync(id);
            if (!ok) return NotFound();
            TempData["Msg"] = $"Inmueble Id {id} eliminado.";
            return RedirectToAction(nameof(Index));
        }

        // ================= helpers =================

        private async Task CargarPropietariosAsync(int? seleccionadoId = null)
        {
            // Opción A (recomendada): un método del repo que ya devuelva (Id, Texto) con JOIN
            // var items = await _repoProp.ListarParaComboAsync(); // Id, Texto (Apellido, Nombre)
            // ViewBag.Propietarios = items.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Texto, Selected = (seleccionadoId == x.Id) }).ToList();

            // Opción B: si aún no tenés ListarParaComboAsync(), uso ObtenerTodos() y armo el texto aquí:
            var props = await _repoProp.ListarAsync(); // ajusta al nombre real de tu método
            ViewBag.Propietarios = props
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Apellido}, {p.Nombre}",
                    Selected = (seleccionadoId == p.Id)
                })
                .OrderBy(s => s.Text)
                .ToList();
        }
    }
}