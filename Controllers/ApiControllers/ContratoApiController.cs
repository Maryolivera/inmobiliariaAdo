using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using InmobiliariaAdo.Data;
using InmobiliariaAdo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ContratoApiController : ControllerBase
{
    private readonly ContratoRepositorio _contratos;
    private readonly PagoRepositorio _pagos;
    private readonly InmuebleRepositorio _inmuebles;

    public ContratoApiController(
        ContratoRepositorio contratos,
    
        InmuebleRepositorio inmuebles)
    {
        _contratos = contratos;
    
        _inmuebles = inmuebles;
    }

    // GET /api/ContratoApi/inmueble/{idInmueble}
    [HttpGet("inmueble/{idInmueble}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<List<Contrato>>> ListarContratosPorInmueble(int idInmueble)
    {
        // 1Propietario desde el token
        var claim = User.FindFirst("propietarioId");
        if (claim is null || !int.TryParse(claim.Value, out var propietarioId))
            return Unauthorized("Token inválido o sin propietarioId.");

        // 2 Validar que el inmueble pertenezca al propietario autenticado
        var inmueble = await _inmuebles.ObtenerPorIdAsync(idInmueble);
        if (inmueble is null || inmueble.PropietarioId != propietarioId)
            return NotFound("Inmueble no encontrado o no pertenece al propietario autenticado.");

        // 3 Obtener contratos
        var lista = await _contratos.ListarPorInmuebleAsync(idInmueble);
        return Ok(lista ?? new List<Contrato>());
    }

   

}
