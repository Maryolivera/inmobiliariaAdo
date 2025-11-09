using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InmobiliariaAdo.Data;
using InmobiliariaAdo.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;

[ApiController]
[Route("api/[controller]")]
public class PropietariosApiController : ControllerBase
{
    private readonly IConfiguration _cfg;
    private readonly UsuarioRepositorio _usuarios;
    private readonly PropietarioRepositorio _props;

    public PropietariosApiController(IConfiguration cfg, UsuarioRepositorio usuarios, PropietarioRepositorio props)
    {
        _cfg = cfg;
        _usuarios = usuarios;
        _props = props;
    }

    // POST /api/PropietariosApi/login  (x-www-form-urlencoded)
    [HttpPost("login")]
    [AllowAnonymous]
    [Consumes("application/x-www-form-urlencoded")]
    [Produces("text/plain")]
    public async Task<IActionResult> Login(
        [FromForm] string Usuario,   // email
        [FromForm] string Clave)     // password en texto plano
    {
        var email = (Usuario ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(Clave))
            return BadRequest("Faltan credenciales.");

        var user = await _usuarios.ValidarLoginAsync(email, Clave);
        if (user is null)
            return Unauthorized("Credenciales inválidas.");

        var prop = await _props.ObtenerPorEmailAsync(email);
        if (prop is null)
            return Unauthorized("No existe propietario asociado al email.");

        var jwtSec = _cfg.GetSection("Jwt");
        var keyStr   = jwtSec["Key"]      ?? throw new InvalidOperationException("Falta Jwt:Key en appsettings.json");
        var issuer   = jwtSec["Issuer"]   ?? throw new InvalidOperationException("Falta Jwt:Issuer en appsettings.json");
        var audience = jwtSec["Audience"] ?? throw new InvalidOperationException("Falta Jwt:Audience en appsettings.json");
        var minsStr  = jwtSec["ExpireMinutes"] ?? throw new InvalidOperationException("Falta Jwt:ExpireMinutes en appsettings.json");

        if (!int.TryParse(minsStr, out var expireMinutes))
            expireMinutes = 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("propietarioId", prop.Id.ToString()),
            new Claim("tipo", "Propietario")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return Content(jwt, "text/plain");
    }

    // GET /api/PropietariosApi  (perfil)
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult<Propietario>> GetPerfil()
    {
        var propietarioIdClaim = User.FindFirst("propietarioId");
        if (propietarioIdClaim is null)
            return Unauthorized();

        if (!int.TryParse(propietarioIdClaim.Value, out var propietarioId))
            return BadRequest("El identificador de propietario en el token no es válido.");

        var p = await _props.ObtenerPorIdAsync(propietarioId);
        if (p is null)
            return NotFound();

        return Ok(p);
    }

    // PUT /api/PropietariosApi/actualizar
    [HttpPut("actualizar")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Actualizar([FromBody] Propietario dto)
    {
        var propietarioId = int.Parse(User.FindFirst("propietarioId")!.Value);

        var propietario = await _props.ObtenerPorIdAsync(propietarioId);
        if (propietario is null)
            return NotFound("Propietario no encontrado.");

        propietario.DNI = dto.DNI;
        propietario.Nombre = dto.Nombre;
        propietario.Apellido = dto.Apellido;
        propietario.Telefono = dto.Telefono;
        propietario.Domicilio = dto.Domicilio;
        propietario.Email = dto.Email;

        await _props.ActualizarAsync(propietario);
        return NoContent();
    }

    // PUT /api/PropietariosApi/changePassword  (x-www-form-urlencoded)
    [HttpPut("changePassword")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> ChangePassword([FromForm] string ClaveActual, [FromForm] string ClaveNueva)
    {
        var email = User.FindFirst(ClaimTypes.Email)!.Value;

        var usuario = await _usuarios.ValidarLoginAsync(email, ClaveActual);
        if (usuario is null)
            return Unauthorized("Clave actual inválida.");

        var ok = await _usuarios.CambiarClaveAsync(usuario.Id, ClaveNueva);
        if (!ok)
            return StatusCode(500, "No se pudo actualizar la contraseña.");

        var verif = await _usuarios.ValidarLoginAsync(email, ClaveNueva);
        if (verif is null)
            return StatusCode(500, "No se pudo confirmar el cambio de contraseña.");

        return NoContent();
    }
}
