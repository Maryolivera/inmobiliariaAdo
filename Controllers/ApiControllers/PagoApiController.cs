using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using InmobiliariaAdo.Data;
using InmobiliariaAdo.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")] // Ruta base: /api/PagoApi
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Requiere token JWT
public class PagoApiController : ControllerBase
{
    private readonly PagoRepositorio _pagos;
    private readonly ContratoRepositorio _contratos;
    private readonly InmuebleRepositorio _inmuebles;

    public PagoApiController(PagoRepositorio pagos, ContratoRepositorio contratos, InmuebleRepositorio inmuebles)
    {
        _pagos = pagos;
        _contratos = contratos;
        _inmuebles = inmuebles;
    }

    // GET /api/PagoApi/contrato/1
    [HttpGet("contrato/{idContrato}")]
    public async Task<ActionResult<List<Pago>>> ListarPagosPorContrato(int idContrato)
    {
        // Propietario desde el token
        var claim = User.FindFirst("propietarioId");
        if (claim is null || !int.TryParse(claim.Value, out var propietarioId))
            return Unauthorized("Token inválido o sin propietarioId.");

        //Verificar que el contrato pertenezca al propietario
        var contrato = await _contratos.ObtenerPorIdAsync(idContrato);
        if (contrato is null)
            return NotFound("Contrato no encontrado.");

        var inmueble = await _inmuebles.ObtenerPorIdAsync(contrato.InmuebleId);
        if (inmueble is null || inmueble.PropietarioId != propietarioId)
            return Forbid("El contrato no pertenece al propietario autenticado.");

        // Obtener pagos del contrato
        var lista = await _pagos.ListarPorContratoAsync(idContrato);

        // devolver resultado
        return Ok(lista ?? new List<Pago>());
    }
}
