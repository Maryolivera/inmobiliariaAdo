using InmobiliariaAdo.Data;
using InmobiliariaAdo.Models;
using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaAdo.Controllers
{
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

    var id = await _repo.CrearAsync(x);

    // NUEVO: armo mensaje con persona
    var info = await _repo.ObtenerResumenPagoAsync(id);
    TempData["Msg"] = $"Pago N° {info.Numero} de {info.InquilinoNombre} registrado.";

    return RedirectToAction(nameof(Index), new { contratoId = x.ContratoId });
}

         // GET /Pagos/Edit/5  (editar solo concepto)
public async Task<IActionResult> Edit(int id)
{
    var pago = await _repo.ObtenerPorIdAsync(id);
    if (pago == null) return NotFound();
    if (pago.Anulado) {
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

            // NUEVO: armo mensaje con persona
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


    }
}
