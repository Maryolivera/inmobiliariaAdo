using System;
using System.Linq;
using System.Threading.Tasks;
using InmobiliariaAdo.Data;
using InmobiliariaAdo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using InmobiliariaAdo.Services; // 👈 para usar AuditoriaArchivo


namespace InmobiliariaAdo.Controllers
{
    
    [Authorize] // 🔒 restringido a usuarios logueados
    public class ContratosController : Controller
    {
        private readonly ContratoRepositorio _repo;
        private readonly InmuebleRepositorio _repoInmuebles;
        private readonly InquilinoRepositorio _repoInquilinos;

        public ContratosController(
            ContratoRepositorio repo,
            InmuebleRepositorio repoInmuebles,
            InquilinoRepositorio repoInquilinos)
        {
            _repo = repo;
            _repoInmuebles = repoInmuebles;
            _repoInquilinos = repoInquilinos;
        }

        // GET /Contratos
        public async Task<IActionResult> Index()
        {
            ViewBag.DiasActual = 30; // valor por defecto del selector
            var lista = await _repo.ListarAsync();
            return View(lista);
        }

        // GET /Contratos/Create
        public async Task<IActionResult> Create()
        {
            await CargarCombosAsync();
            return View(new Contrato
            {
                FechaInicio = DateTime.Today,
                FechaFin = DateTime.Today.AddYears(1)
            });
        }

        // POST /Contratos/Create
       // POST /Contratos/Create
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Contrato x)
{
    if (!ModelState.IsValid)
    {
        await CargarCombosAsync();
        return View(x);
    }

    if (x.FechaInicio >= x.FechaFin)
    {
        ModelState.AddModelError("", "La fecha de inicio debe ser anterior a la de fin.");
        await CargarCombosAsync();
        return View(x);
    }

    if (await _repo.ExisteSolapeAsync(x.InmuebleId, x.FechaInicio, x.FechaFin, null))
    {
        ModelState.AddModelError("", "El inmueble está ocupado en esas fechas.");
        await CargarCombosAsync();
        return View(x);
    }

    // 1) Crear contrato en la BD
    var id = await _repo.CrearAsync(x);

    // 2) AUDITORÍA: registrar quién lo creó
    var userName = User.Identity?.Name ?? "Desconocido";
    AuditoriaArchivo.RegistrarContrato(id, "creado", userName);

    // 3) Mensaje de confirmación
    TempData["Msg"] = $"Contrato creado (Id {id}).";

    // 4) Redirigir al listado
    return RedirectToAction(nameof(Index));
}

        // GET /Contratos/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var x = await _repo.ObtenerPorIdAsync(id);
            if (x == null) return NotFound();
            await CargarCombosAsync();
            return View(x);
        }

        // POST /Contratos/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Contrato x)
        {
            if (!ModelState.IsValid)
            {
                await CargarCombosAsync();
                return View(x);
            }

            if (x.FechaInicio >= x.FechaFin)
            {
                ModelState.AddModelError("", "La fecha de inicio debe ser anterior a la de fin.");
                await CargarCombosAsync();
                return View(x);
            }

            if (await _repo.ExisteSolapeAsync(x.InmuebleId, x.FechaInicio, x.FechaFin, x.Id))
            {
                ModelState.AddModelError("", "El inmueble está ocupado en esas fechas.");
                await CargarCombosAsync();
                return View(x);
            }

            var ok = await _repo.ActualizarAsync(x);
            if (!ok) return NotFound();

            TempData["Msg"] = $"Contrato Id {x.Id} actualizado.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Contratos/Delete/5
        [Authorize(Policy = "EsAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _repo.ObtenerPorIdAsync(id);
            if (x == null) return NotFound();
            return View(x);
        }

        // POST /Contratos/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
         [Authorize(Policy = "EsAdmin")]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ok = await _repo.EliminarAsync(id);
            if (!ok) return NotFound();

            TempData["Msg"] = $"Contrato Id {id} eliminado.";
            return RedirectToAction(nameof(Index));
        }

       
// GET /Contratos/Details/5
public async Task<IActionResult> Details(int id)
{
    var x = await _repo.ObtenerPorIdAsync(id);
    if (x == null) return NotFound();

    // 🔎 Auditoría
    var a = AuditoriaArchivo.ObtenerAuditoriaContrato(id);
    ViewBag.CreadoPor = a.CreadoPor;
    ViewBag.FechaCreado = a.FechaCreado?.ToString("g");
    ViewBag.TerminadoPor = a.TerminadoPor;
    ViewBag.FechaTerminado = a.FechaTerminado?.ToString("g");

    return View(x); // busca Views/Contratos/Details.cshtml
}


        // GET /Contratos/Terminar/5
        public async Task<IActionResult> Terminar(int id)
        {
            var c = await _repo.ObtenerPorIdAsync(id);
            if (c == null) return NotFound();
            return View("Terminar", c);
        }

        // POST /Contratos/Terminar/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Terminar(int id, DateTime fechaEfectiva)
        {
            var c = await _repo.ObtenerPorIdAsync(id);
            if (c == null) return NotFound();

            var totalMeses = Math.Max(1,
                ((c.FechaFin.Year - c.FechaInicio.Year) * 12) + c.FechaFin.Month - c.FechaInicio.Month);

            var cumplidos = Math.Max(0,
                ((fechaEfectiva.Year - c.FechaInicio.Year) * 12) + fechaEfectiva.Month - c.FechaInicio.Month);

            var mesesMulta = (cumplidos < totalMeses / 2.0) ? 2 : 1;
            var multa = mesesMulta * c.MontoMensual;

            await _repo.TerminarAnticipadoAsync(id, fechaEfectiva);

            TempData["Msg"] = $"Contrato terminado. Multa sugerida: ${multa:0.##} (meses: {mesesMulta}).";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET /Contratos/Renovar/5
        public async Task<IActionResult> Renovar(int id)
        {
            var c = await _repo.ObtenerPorIdAsync(id);
            if (c == null) return NotFound();
            return View("Renovar", c);
        }

        // POST /Contratos/Renovar/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Renovar(int id, DateTime nuevoInicio, DateTime nuevoFin, decimal nuevoMonto)
        {
            if (nuevoInicio >= nuevoFin)
            {
                TempData["Msg"] = "Rango de fechas inválido.";
                return RedirectToAction(nameof(Renovar), new { id });
            }

            var nuevoId = await _repo.RenovarAsync(id, nuevoInicio, nuevoFin, nuevoMonto);
            TempData["Msg"] = $"Contrato renovado (nuevo Id {nuevoId}).";
            return RedirectToAction(nameof(Index));
        }

        // GET /Contratos/Vigentes
        public async Task<IActionResult> Vigentes()
        {
            ViewBag.DiasActual = 30; // mantiene el selector
            var lista = await _repo.ListarVigentesAsync(DateTime.Today);
            TempData["Msg"] = "Contratos vigentes hoy:";
            return View("Index", lista);
        }

        // GET /Contratos/TerminanEn?dias=30
        public async Task<IActionResult> TerminanEn(int dias = 30)
        {
            // rangos no superpuestos
            int min = 0, max = dias;
            if (dias == 60) { min = 31; max = 60; }
            else if (dias == 90) { min = 61; max = 90; }


            var lista = await _repo.ListarQueTerminanEnRangoAsync(min, max);

            ViewBag.DiasActual = dias;

            if (lista == null || !lista.Any())
            {
                TempData["Msg"] = $"No hay contratos que terminen entre {min} y {max} días.";
                return View("Index", new System.Collections.Generic.List<Contrato>());
            }

            TempData["Msg"] = $"Contratos que terminan entre {min} y {max} días:";
            return View("Index", lista);
        }








        // ================= helpers =================
        private async Task CargarCombosAsync()
        {
            var inmuebles = await _repoInmuebles.ListarAsync();
            ViewBag.Inmuebles = inmuebles
                .Select(i => new SelectListItem { Value = i.Id.ToString(), Text = i.Direccion })
                .OrderBy(x => x.Text)
                .ToList();

            var inquilinos = await _repoInquilinos.ListarAsync();
            ViewBag.Inquilinos = inquilinos
                .Select(q => new SelectListItem { Value = q.Id.ToString(), Text = $"{q.Apellido}, {q.Nombre}" })
                .OrderBy(x => x.Text)
                .ToList();
        }
    }
}

