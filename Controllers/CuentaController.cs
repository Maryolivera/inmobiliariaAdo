using System.Security.Claims;
using InmobiliariaAdo.Data;
using InmobiliariaAdo.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaAdo.Controllers
{
    public class CuentaController : Controller
    {
        private readonly UsuarioRepositorio _repo;
        public CuentaController(UsuarioRepositorio repo) => _repo = repo;

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string clave, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(clave))
            {
                TempData["Msg"] = "Email y clave son obligatorios.";
                return View();
            }

            var u = await _repo.ValidarLoginAsync(email, clave);
            if (u == null)
            {
                TempData["Msg"] = "Credenciales inválidas.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()),
                new Claim(ClaimTypes.Name, u.Email),
                new Claim(ClaimTypes.GivenName, u.Nombre + " " + u.Apellido),
                new Claim(ClaimTypes.Role, u.Rol)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public IActionResult Denegado() => View();

        // ================= PERFIL =================

        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            var idLog = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var usuario = await _repo.BuscarPorIdAsync(idLog);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(Usuario u)
        {
            var idLog = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var esAdmin = User.IsInRole("Administrador");
            if (!esAdmin) u.Id = idLog; // un empleado solo puede editar su propio perfil

            var ok = await _repo.ActualizarPerfilAsync(u);
            TempData["Msg"] = ok ? "Perfil actualizado con éxito." : "Error al actualizar el perfil.";
            return RedirectToAction(nameof(Perfil));
        }

        // ================= CAMBIAR CLAVE =================

        [Authorize]
        public IActionResult CambiarClave() => View();

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarClave(string claveActual, string nuevaClave, string confirmarClave)
        {
            if (string.IsNullOrWhiteSpace(claveActual) || string.IsNullOrWhiteSpace(nuevaClave))
            {
                TempData["Msg"] = "Debes completar todos los campos.";
                return View();
            }

            if (nuevaClave != confirmarClave)
            {
                TempData["Msg"] = "La nueva clave y la confirmación no coinciden.";
                return View();
            }

            var idLog = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var u = await _repo.BuscarPorIdAsync(idLog);
            if (u == null)
            {
                TempData["Msg"] = "Usuario no encontrado.";
                return View();
            }

            // Verificar clave actual
            var validado = await _repo.ValidarLoginAsync(u.Email, claveActual);
            if (validado == null)
            {
                TempData["Msg"] = "La clave actual no es correcta.";
                return View();
            }

            // Guardar la nueva clave
            var ok = await _repo.CambiarClaveAsync(idLog, nuevaClave);
            TempData["Msg"] = ok ? "Clave cambiada con éxito." : "Error al cambiar la clave.";
            return View();
        }
    }
}
