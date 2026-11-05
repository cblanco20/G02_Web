using G02_PrograAvanzadaWeb.Models;
using Microsoft.AspNetCore.Mvc;
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

            try
            {
                // Crear el cliente HTTP configurado para la API.
                var cliente = _httpClientFactory.CreateClient("MealPrepAPI");

                
                var respuesta = await cliente.GetAsync("api/Usuarios/Consultar");

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
            catch (Exception)
            {
                TempData["Error"] = "Ocurrió un error al consultar los usuarios.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Usuarios()
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

            try
            {
                // Crear el cliente HTTP configurado para la API.
                var cliente = _httpClientFactory.CreateClient("MealPrepAPI");

                // Consultar los usuarios registrados.
                var respuesta = await cliente.GetAsync("api/Usuarios/Consultar");

                if (!respuesta.IsSuccessStatusCode)
                {
                    TempData["Error"] = "No se pudieron consultar los usuarios.";
                    return View(new List<UsuarioConsulta>());
                }

                // Convertir la respuesta JSON en una lista de usuarios.
                var usuarios = await respuesta.Content
                    .ReadFromJsonAsync<List<UsuarioConsulta>>();

                return View(usuarios ?? new List<UsuarioConsulta>());
            }
            catch (Exception)
            {
                TempData["Error"] = "Ocurrió un error al conectar con la API.";

                return View(new List<UsuarioConsulta>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int idUsuario, bool activo)
        {
            // Verificar que el usuario haya iniciado sesión.
            if (HttpContext.Session.GetInt32("Autenticado") != 1)
            {
                return RedirectToAction("Login", "Cuenta");
            }

            // Permitir la operación únicamente al Administrador.
            if (HttpContext.Session.GetInt32("IdRol") != 2)
            {
                return StatusCode(403);
            }

            // Validar el identificador del usuario.
            if (idUsuario <= 0)
            {
                TempData["Error"] = "El usuario seleccionado no es válido.";
                return RedirectToAction("Usuarios");
            }

            // Evitar que el administrador modifique su propia cuenta.
            if (idUsuario == HttpContext.Session.GetInt32("IdUsuario"))
            {
                TempData["Error"] = "No puedes modificar el estado de tu propia cuenta.";
                return RedirectToAction("Usuarios");
            }

            try
            {
                // Crear el cliente HTTP configurado para la API.
                var cliente = _httpClientFactory.CreateClient("MealPrepAPI");

                // Preparar los datos que recibirá la API.
                var datos = new
                {
                    IdUsuario = idUsuario,
                    Activo = activo
                };

                // Enviar la solicitud al método CambiarEstado de la API.
                var respuesta = await cliente.PutAsJsonAsync(
                    "api/Usuarios/CambiarEstado",
                    datos
                );

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
            catch (Exception)
            {
                TempData["Error"] = "Ocurrió un error al conectar con la API.";
            }

            return RedirectToAction("Usuarios");
        }
    }
}