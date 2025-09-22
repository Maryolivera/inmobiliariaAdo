using InmobiliariaAdo.Data;
using InmobiliariaAdo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InmobiliariaAdo.Services; // 👈 para poder usar AuditoriaArchivo


namespace InmobiliariaAdo.Controllers
{
     [Authorize] // 🔒 restringido a usuarios logueados
    public class PagosController : Controller
    {
        private readonly PagoRepositorio _repo;
        public PagosController(PagoRepositorio repo) => _repo = repo;

        // GET /Pagos?contratoId=5


        // GET /Pagos/Create?contratoId=5
        public async Task<IActionResult> Create(int contratoId)
        {
            var prox = await _repo.ObtenerSiguienteNumeroAsync(contratoId);
            var pago = new Pago
            {
                ContratoId = contratoId,
                Numero = prox,
                FechaPago = DateTime.Today
            };
            return View(pago);
        }


        // POST /Pagos/Create
       [HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Pago x)
{
    if (!ModelState.IsValid) return View(x);

    // 1) Crear el pago en la BD
    var id = await _repo.CrearAsync(x);

    // 2) Registrar en la auditoría quién lo creó
    var userName = User.Identity?.Name ?? "Desconocido";
    AuditoriaArchivo.RegistrarPago(id, "creado", userName);

    // 3) Mensaje para mostrar en la UI
    var info = await _repo.ObtenerResumenPagoAsync(id);
    TempData["Msg"] = $"Pago N° {info.Numero} de {info.InquilinoNombre} registrado.";

    // 4) Redirigir al listado de pagos del contrato
    return RedirectToAction(nameof(Index), new { contratoId = x.ContratoId });
}


        // GET /Pagos/Edit/5  (editar solo concepto)
        public async Task<IActionResult> Edit(int id)
        {
            var pago = await _repo.ObtenerPorIdAsync(id);
            if (pago == null) return NotFound();
            if (pago.Anulado)
            {
                TempData["Msg"] = "No se puede editar un pago anulado.";
                var contratoId = await _repo.ObtenerContratoIdAsync(id);
                return RedirectToAction(nameof(Index), new { contratoId });
            }
            return View(pago);
        }

        // POST /Pagos/Edit/5  (guardar solo concepto)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ContratoId,Detalle")] Pago x)
        {
            if (id != x.Id) return NotFound();
            if (!ModelState.IsValid) return View(x);

            var ok = await _repo.EditarConceptoAsync(id, x.Detalle);
            if (!ok) return NotFound();

            // NUEVO: armo mensaje con persona
            var info = await _repo.ObtenerResumenPagoAsync(id);
            TempData["Msg"] = $"Pago N° {info.Numero} de {info.InquilinoNombre} actualizado.";

            return RedirectToAction(nameof(Index), new { contratoId = x.ContratoId });
        }



        // GET /Pagos/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var x = await _repo.ObtenerPorIdAsync(id);
            if (x == null) return NotFound();
            return View(x);
        }

        // POST /Pagos/Delete/5  (marca como Anulado)
       [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    var ok = await _repo.AnularAsync(id);
    if (!ok) return NotFound();

    // 🔎 AUDITORÍA: anulado por
    var userName = User.Identity?.Name ?? "Desconocido";
    AuditoriaArchivo.RegistrarPago(id, "anulado", userName);

    var info = await _repo.ObtenerResumenPagoAsync(id);
    TempData["Msg"] = $"Pago N° {info.Numero} de {info.InquilinoNombre} anulado.";

    return RedirectToAction(nameof(Index), new { contratoId = info.ContratoId });
}

        public async Task<IActionResult> Index(int contratoId)
        {
            var lista = await _repo.ListarPorContratoAsync(contratoId);
            ViewBag.ContratoId = contratoId;

            // NUEVO: nombre del inquilino para mostrar en la tabla
            ViewBag.InquilinoNombre = await _repo.ObtenerInquilinoNombrePorContratoAsync(contratoId);

            return View(lista);
        }

        // GET: /Pagos/Details/5
       // GET: /Pagos/Details/5
public async Task<IActionResult> Details(int id)
{
    var x = await _repo.ObtenerPorIdAsync(id);
    if (x == null) return NotFound();

    // 🔎 Obtener auditoría
    var a = AuditoriaArchivo.ObtenerAuditoriaPago(id);
    ViewBag.CreadoPor = a.CreadoPor;
    ViewBag.FechaCreado = a.FechaCreado?.ToString("g");
    ViewBag.AnuladoPor = a.AnuladoPor;
    ViewBag.FechaAnulado = a.FechaAnulado?.ToString("g");

    return View(x);
}



    }
}
