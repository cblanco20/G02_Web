using G02_PrograAvanzadaWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace G02_PrograAvanzadaWeb.Controllers
{
    public class AdministradorController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdministradorController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null)
            {
                return acceso;
            }

            try
            {
                using var solicitud = CrearSolicitud(HttpMethod.Get, "api/Usuarios/Consultar");
                using var respuesta = await _httpClientFactory
                    .CreateClient("MealPrepAPI")
                    .SendAsync(solicitud);

                if (EsSesionVencida(respuesta))
                {
                    return CerrarPorVencimiento();
                }

                if (!respuesta.IsSuccessStatusCode)
                {
                    TempData["Error"] = "No se pudo consultar la cantidad de usuarios.";
                    return View();
                }

                var usuarios = await respuesta.Content
                    .ReadFromJsonAsync<List<UsuarioConsulta>>();

                ViewBag.TotalUsuarios = usuarios?.Count ?? 0;

                return View();
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Ocurrió un error al consultar los usuarios.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Usuarios()
        {
            var acceso = ValidarAcceso();
            if (acceso != null)
            {
                return acceso;
            }

            try
            {
                using var solicitud = CrearSolicitud(HttpMethod.Get, "api/Usuarios/Consultar");
                using var respuesta = await _httpClientFactory
                    .CreateClient("MealPrepAPI")
                    .SendAsync(solicitud);

                if (EsSesionVencida(respuesta))
                {
                    return CerrarPorVencimiento();
                }

                if (!respuesta.IsSuccessStatusCode)
                {
                    TempData["Error"] = "No se pudieron consultar los usuarios.";
                    return View(new List<UsuarioConsulta>());
                }

                var usuarios = await respuesta.Content
                    .ReadFromJsonAsync<List<UsuarioConsulta>>();

                return View(usuarios ?? new List<UsuarioConsulta>());
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Ocurrió un error al conectar con la API.";
                return View(new List<UsuarioConsulta>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int idUsuario, bool activo)
        {
            var acceso = ValidarAcceso();
            if (acceso != null)
            {
                return acceso;
            }

            if (idUsuario <= 0)
            {
                TempData["Error"] = "El usuario seleccionado no es válido.";
                return RedirectToAction("Usuarios");
            }

            if (idUsuario == HttpContext.Session.GetInt32("IdUsuario"))
            {
                TempData["Error"] = "No puedes modificar el estado de tu propia cuenta.";
                return RedirectToAction("Usuarios");
            }

            try
            {
                using var solicitud = CrearSolicitud(HttpMethod.Put, "api/Usuarios/CambiarEstado");
                solicitud.Content = JsonContent.Create(new
                {
                    IdUsuario = idUsuario,
                    Activo = activo
                });

                using var respuesta = await _httpClientFactory
                    .CreateClient("MealPrepAPI")
                    .SendAsync(solicitud);

                if (EsSesionVencida(respuesta))
                {
                    return CerrarPorVencimiento();
                }

                if (respuesta.IsSuccessStatusCode)
                {
                    TempData["Exito"] = activo
                        ? "El usuario fue activado correctamente."
                        : "El usuario fue desactivado correctamente.";
                }
                else
                {
                    TempData["Error"] = "No se pudo cambiar el estado del usuario.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Ocurrió un error al conectar con la API.";
            }

            return RedirectToAction("Usuarios");
        }

        private IActionResult? ValidarAcceso()
        {
            if (HttpContext.Session.GetInt32("Autenticado") != 1)
            {
                return RedirectToAction("Login", "Cuenta");
            }

            if (HttpContext.Session.GetInt32("IdRol") != 2)
            {
                return StatusCode(403);
            }

            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("TokenJWT")))
            {
                return CerrarPorVencimiento();
            }

            return null;
        }

        private HttpRequestMessage CrearSolicitud(HttpMethod metodo, string ruta)
        {
            var solicitud = new HttpRequestMessage(metodo, ruta);

            solicitud.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                HttpContext.Session.GetString("TokenJWT"));

            return solicitud;
        }

        private static bool EsSesionVencida(HttpResponseMessage respuesta)
        {
            return respuesta.StatusCode == HttpStatusCode.Unauthorized
                || respuesta.StatusCode == HttpStatusCode.Forbidden;
        }

        private IActionResult CerrarPorVencimiento()
        {
            HttpContext.Session.Clear();
            TempData["Error"] = "Tu sesión ha vencido. Inicia sesión nuevamente.";
            return RedirectToAction("Login", "Cuenta");
        }
    }
}
