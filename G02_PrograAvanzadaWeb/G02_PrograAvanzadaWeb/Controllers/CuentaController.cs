using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace G02_PrograAvanzadaWeb.Controllers
{
    public class CuentaController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Registro()
        {
            return View();
        }

        public IActionResult RecuperarAcceso()
        {
            return View();
        }
    }
}
