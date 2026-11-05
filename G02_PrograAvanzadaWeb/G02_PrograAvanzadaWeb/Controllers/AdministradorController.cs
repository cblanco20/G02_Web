using Microsoft.AspNetCore.Mvc;

namespace G02_PrograAvanzadaWeb.Controllers
{
    public class AdministradorController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            // Verificar que el usuario haya iniciado sesión.
            if (HttpContext.Session.GetInt32("Autenticado") != 1)
            {
                return RedirectToAction("Login", "Cuenta");
            }

            // Permitir el acceso únicamente al Administrador.
            if (HttpContext.Session.GetInt32("IdRol") != 2)
            {
                return StatusCode(403);
            }

            return View();
        }
    }
}