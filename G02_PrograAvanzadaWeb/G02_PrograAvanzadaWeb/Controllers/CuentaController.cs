using G02_PrograAvanzadaWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace G02_PrograAvanzadaWeb.Controllers
{
    public class CuentaController : Controller
    {
        private readonly HttpClient _httpClient;

        public CuentaController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("MealPrepAPI");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "api/Usuarios/Login", model);

                if (response.IsSuccessStatusCode)
                {
                    var datos = await response.Content
                        .ReadFromJsonAsync<UsuarioResponse>();

                    if (datos == null)
                    {
                        ViewBag.Mensaje = "No se pudo obtener la información del usuario.";
                        return View(model);
                    }

                    HttpContext.Session.Clear();

                    HttpContext.Session.SetInt32("Autenticado", 1);
                    HttpContext.Session.SetInt32("IdUsuario", datos.IdUsuario);
                    HttpContext.Session.SetString("Nombre", datos.NombreUsuario);
                    HttpContext.Session.SetString("Rol", datos.NombreRol);
                    HttpContext.Session.SetInt32("IdRol", datos.IdRol);

                    if (datos.IdRol == 1)
                    {
                        return RedirectToAction("Index", "Usuario");
                    }

                    if (datos.IdRol == 2)
                    {
                        return RedirectToAction("Index", "Administrador");
                    }

                    HttpContext.Session.Clear();

                    ViewBag.Mensaje = "El usuario no tiene un rol válido.";
                    return View(model);
                }

                var resultado = await response.Content
                    .ReadFromJsonAsync<UsuarioResponse>();

                ViewBag.Mensaje = resultado?.Mensaje
                    ?? "Correo o contraseña incorrectos.";
            }
            catch (HttpRequestException)
            {
                ViewBag.Mensaje =
                    "No fue posible conectar con el servidor. Inténtelo nuevamente.";
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Registro(RegistroUsuario model)
        {
            if (!model.AceptaTerminos)
            {
                ModelState.AddModelError(
                    nameof(model.AceptaTerminos),
                    "Debe aceptar los términos y condiciones.");
            }

            if (!ModelState.IsValid)
                return View(model);
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "api/Usuarios/Registrar",
                    new
                    {
                        model.NombreUsuario,
                        model.Correo,
                        model.Contrasena
                    });

                if (response.IsSuccessStatusCode)
                {
                    TempData["MensajeExito"] =
                        "Cuenta creada correctamente. Ya puede iniciar sesión.";

                    return RedirectToAction("Login", "Cuenta");
                }

                var datos = await response.Content
                    .ReadFromJsonAsync<UsuarioResponse>();

                ViewBag.Mensaje = datos?.Mensaje
                    ?? "No se pudo registrar el usuario.";
            }
            catch (HttpRequestException)
            {
                ViewBag.Mensaje =
                    "No fue posible conectar con el servidor. Inténtelo nuevamente.";
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult RecuperarAcceso()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuperarAcceso(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                ViewBag.Error = "Debe ingresar su correo electrónico.";
                return View();
            }

            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync(
                    "api/Usuarios/SolicitarRecuperacion",
                    new
                    {
                        Correo = correo
                    }
                );

                if (respuesta.IsSuccessStatusCode)
                {
                    ViewBag.Mensaje =
                        "Si el correo está registrado y activo, " +
                        "recibirá las instrucciones para recuperar su contraseña.";
                }
                else
                {
                    ViewBag.Error =
                        "No fue posible procesar la solicitud. Inténtelo nuevamente.";
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.Error =
                    "No fue posible conectar con el servidor. Inténtelo nuevamente.";
            }

            return View();
        }


        [HttpGet]
        public IActionResult RestablecerContrasena(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("RecuperarAcceso", "Cuenta");
            }

            ViewBag.Token = token;

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestablecerContrasena(
            string token,
            string nuevaContrasena,
            string confirmarContrasena)
        {
            ViewBag.Token = token;

            if (string.IsNullOrWhiteSpace(token))
            {
                ViewBag.Error = "El enlace de recuperación no es válido.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(nuevaContrasena) ||
                nuevaContrasena.Length < 8)
            {
                ViewBag.Error = "La contraseña debe tener al menos 8 caracteres.";
                return View();
            }

            if (nuevaContrasena != confirmarContrasena)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View();
            }

            try
            {
                var respuesta = await _httpClient.PostAsJsonAsync(
                    "api/Usuarios/RestablecerContrasena",
                    new
                    {
                        Token = token,
                        NuevaContrasena = nuevaContrasena
                    }
                );

                if (respuesta.IsSuccessStatusCode)
                {
                    TempData["MensajeExito"] =
                        "Su contraseña se restableció correctamente. Ya puede iniciar sesión.";

                    return RedirectToAction("Login", "Cuenta");
                }

                ViewBag.Error =
                    "No fue posible restablecer la contraseña. " +
                    "El enlace puede ser inválido o haber vencido.";
            }
            catch (HttpRequestException)
            {
                ViewBag.Error =
                    "No fue posible conectar con el servidor. Inténtelo nuevamente.";
            }

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Cuenta");
        }
    }
}