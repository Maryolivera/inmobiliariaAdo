using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Text.Json;
using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InmobiliariaAdo.Data;
using InmobiliariaAdo.Models;

namespace InmobiliariaAdo.Controllers.ApiControllers
{
    public class InmuebleEstadoDto
    {
        public bool Suspendido { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")] // /api/Inmuebles
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class InmueblesApiController : ControllerBase
    {
        private readonly InmuebleRepositorio _repo;

        public InmueblesApiController(InmuebleRepositorio repo)
        {
            _repo = repo;
        }

        private int GetPropietarioId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "propietarioId");
            if (claim == null) throw new UnauthorizedAccessException("Token sin propietarioId.");
            return int.Parse(claim.Value);
        }

        // GET /api/Inmuebles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inmueble>>> GetMisInmuebles()
        {
            var propietarioId = GetPropietarioId();
            var lista = await _repo.ObtenerPorPropietarioAsync(propietarioId);
            return Ok(lista);
        }

        // GET /api/Inmuebles/GetContratoVigente
        [HttpGet("GetContratoVigente")]
        public async Task<ActionResult<IEnumerable<Inmueble>>> GetConContratoVigente()
        {
            var propietarioId = GetPropietarioId();
            var lista = await _repo.ObtenerConContratoVigenteAsync(propietarioId);
            return Ok(lista);
        }

        // PUT /api/Inmuebles/actualizar
        [HttpPut("actualizar")]
        public async Task<ActionResult<Inmueble>> Actualizar([FromBody] Inmueble dto)
        {
            var propietarioId = GetPropietarioId();

            var db = await _repo.ObtenerPorIdAsync(dto.Id);
            if (db == null) return NotFound("Inmueble no encontrado.");
            if (db.PropietarioId != propietarioId) return Forbid("No es tu inmueble.");

            dto.PropietarioId = propietarioId;

            var ok = await _repo.ActualizarAsync(dto);
            if (!ok) return BadRequest("No se pudo actualizar.");
            return Ok(dto);
        }

        // POST /api/Inmuebles/cargar  (multipart/form-data: imagen + inmueble(JSON))
        [HttpPost("cargar")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Inmueble>> Cargar([FromForm] IFormFile? imagen, [FromForm] string inmueble)
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "propietarioId");
            if (claim == null) return Unauthorized("Token sin propietarioId.");
            if (!int.TryParse(claim.Value, out var propietarioId))
                return BadRequest("Claim propietarioId inválido.");

            Inmueble? entidad;
            try
            {
                entidad = JsonSerializer.Deserialize<Inmueble>(
                    inmueble,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
            }
            catch (JsonException)
            {
                return BadRequest("JSON de 'inmueble' inválido.");
            }
            if (entidad == null) return BadRequest("JSON de 'inmueble' vacío.");

            entidad.PropietarioId = propietarioId;
            entidad.Suspendido = true;

            if (imagen != null && imagen.Length > 0)
            {
                const long MaxBytes = 5 * 1024 * 1024;
                if (imagen.Length > MaxBytes)
                    return BadRequest("La imagen excede el tamaño máximo (5MB).");

                var ext = Path.GetExtension(imagen.FileName).ToLowerInvariant();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(ext))
                    return BadRequest("Formato de imagen no permitido. Use JPG/PNG/WEBP.");

                if (string.IsNullOrWhiteSpace(imagen.ContentType) ||
                    !imagen.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    return BadRequest("El archivo no parece ser una imagen válida.");

                var safeFileName = Path.GetFileName($"{Guid.NewGuid()}{ext}");
                var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(dir);

                var path = Path.Combine(dir, safeFileName);
                await using (var fs = System.IO.File.Create(path))
                {
                    await imagen.CopyToAsync(fs);
                }

                entidad.Portada = $"/uploads/{safeFileName}";
            }

            var id = await _repo.CrearAsync(entidad);
            entidad.Id = id;

            return Ok(entidad);
        }

        // PUT /api/Inmuebles/habilitar/{id}
        [HttpPut("habilitar/{id}")]
        [Consumes("application/json")]
        public async Task<IActionResult> HabilitarDeshabilitar(int id, [FromBody] InmuebleEstadoDto estado)
        {
            if (id <= 0) return BadRequest("Id de inmueble inválido.");
            if (estado is null) return BadRequest("Debe enviar el estado de suspensión.");

            var claim = User.FindFirst("propietarioId");
            if (claim is null || !int.TryParse(claim.Value, out var propietarioId))
                return Unauthorized("Token inválido o sin propietarioId.");

            var inmueble = await _repo.ObtenerPorIdAsync(id);
            if (inmueble is null || inmueble.PropietarioId != propietarioId)
                return NotFound("Inmueble no encontrado o no pertenece al propietario autenticado.");

            var exito = await _repo.CambiarEstadoSuspensionAsync(id, estado.Suspendido);
            if (!exito) return StatusCode(500, "No se pudo actualizar el estado del inmueble.");

            return NoContent();
        }
    }
}
