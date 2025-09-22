using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InmobiliariaAdo.Data;
using InmobiliariaAdo.Models;

namespace InmobiliariaAdo.Controllers
{
    public class CuentaController : Controller
    {
        private readonly UsuarioRepositorio _repo;

        public CuentaController(IConfiguration config)
        {
            _repo = new UsuarioRepositorio(config);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string clave)
        {
            var usuario = await _repo.ValidarLoginAsync(email, clave);
            if (usuario == null)
            {
                ModelState.AddModelError("", "Credenciales inválidas");
                return View();
            }

            // Crear claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim(ClaimTypes.GivenName, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // ✅ Guardar avatar en sesión
            HttpContext.Session.SetString("Avatar", usuario.Avatar ?? "");

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear(); // limpiar sesión
            return RedirectToAction("Login");
        }
    }
}
