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
        public async Task<IActionResult> Index(int contratoId)
        {
            var lista = await _repo.ListarPorContratoAsync(contratoId);
            ViewBag.ContratoId = contratoId;
            return View(lista);
        }

        // GET /Pagos/Create?contratoId=5
        public IActionResult Create(int contratoId)
        {
            var pago = new Pago { ContratoId = contratoId, FechaPago = DateTime.Today };
            return View(pago);
        }

        // POST /Pagos/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pago x)
        {
            if (!ModelState.IsValid) return View(x);
            var id = await _repo.CrearAsync(x);
            TempData["Msg"] = $"Pago registrado (Id {id}).";
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
            TempData["Msg"] = $"Pago Id {id} anulado.";
            var contratoId = await _repo.ObtenerContratoIdAsync(id);
            return RedirectToAction(nameof(Index), new { contratoId });
        }
    }
}
