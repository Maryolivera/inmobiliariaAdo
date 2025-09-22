using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InmobiliariaAdo.Data;
using InmobiliariaAdo.Models;

namespace InmobiliariaAdo.Controllers
{
    [Authorize] // 👈 ahora tanto empleados como admins pueden acceder a su perfil
    public class UsuariosController : Controller
    {
        private readonly UsuarioRepositorio _repo;
        private readonly IConfiguration _config;

        public UsuariosController(IConfiguration config)
        {
            _config = config;
            _repo = new UsuarioRepositorio(config);
        }

        // ================== LISTADO (solo admin) ==================
        [Authorize(Policy = "EsAdmin")]
        public async Task<IActionResult> Index()
        {
            var lista = await _repo.ListarAsync();
            return View(lista);
        }

        // ================== CREAR USUARIO (solo admin) ==================
        [Authorize(Policy = "EsAdmin")]
        public IActionResult Crear()
        {
            return View();
        }

        [Authorize(Policy = "EsAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Usuario u, string clavePlano)
        {
            if (ModelState.IsValid)
            {
                await _repo.CrearAsync(u, clavePlano);
                return RedirectToAction(nameof(Index));
            }
            return View(u);
        }

        // ================== EDITAR USUARIO (solo admin) ==================
        [Authorize(Policy = "EsAdmin")]
        public async Task<IActionResult> Editar(int id)
        {
            var u = await _repo.BuscarPorIdAsync(id);
            if (u == null) return NotFound();
            return View(u);
        }

        [Authorize(Policy = "EsAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Usuario u, IFormFile? avatar, string? eliminarAvatar)
        {
            // Manejo de avatar en edición general
            if (!string.IsNullOrEmpty(eliminarAvatar))
            {
                u.Avatar = null;
            }
            else if (avatar != null && avatar.Length > 0)
            {
                var ext = Path.GetExtension(avatar.FileName).ToLower();
                var fileName = $"{Guid.NewGuid()}{ext}";
                var path = Path.Combine("wwwroot/avatars", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                u.Avatar = fileName;
            }

            if (ModelState.IsValid)
            {
                await _repo.ActualizarAsync(u);
                return RedirectToAction(nameof(Index));
            }
            return View(u);
        }

        // ================== ELIMINAR (solo admin) ==================
        [Authorize(Policy = "EsAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _repo.EliminarAsync(id); // 👈 borrado lógico (Activo=0)
            return RedirectToAction(nameof(Index));
        }

        // ================== PERFIL (Empleado/Admin) ==================
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var email = User.Identity?.Name;
            if (email == null) return RedirectToAction("Login", "Cuenta");

            var usuario = await _repo.BuscarPorEmailAsync(email);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(Usuario model, IFormFile? avatar, string? eliminarAvatar)
        {
            // Buscar usuario real en DB
            var usuario = await _repo.BuscarPorIdAsync(model.Id);
            if (usuario == null) return NotFound();

            // Actualizamos campos
            usuario.Nombre = model.Nombre;
            usuario.Apellido = model.Apellido;

            if (!string.IsNullOrEmpty(eliminarAvatar))
            {
                usuario.Avatar = null;
            }
            else if (avatar != null && avatar.Length > 0)
            {
                var ext = Path.GetExtension(avatar.FileName).ToLower();
                var fileName = $"{Guid.NewGuid()}{ext}";
                var path = Path.Combine("wwwroot/avatars", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                usuario.Avatar = fileName;
            }

            await _repo.ActualizarPerfilAsync(usuario);
            return RedirectToAction("Perfil");
        }
    }
}
