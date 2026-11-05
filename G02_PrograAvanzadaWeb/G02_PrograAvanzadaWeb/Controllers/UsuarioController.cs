using Microsoft.AspNetCore.Mvc;

namespace G02_PrograAvanzadaWeb.Controllers
{
    public class UsuarioController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            // Verificar si existe una sesión activa.
            if (HttpContext.Session.GetInt32("Autenticado") != 1)
            {
                return RedirectToAction("Login", "Cuenta");
            }

            // Verificar que tenga el rol de Usuario.
            if (HttpContext.Session.GetInt32("IdRol") != 1)
            {
                return StatusCode(403);
            }

            return View();
        }
    }
}