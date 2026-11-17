using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using G02_PrograAvanzadaWeb.Models;

namespace G02_PrograAvanzadaWeb.Controllers
{
    public abstract class ControladorConSesion : Controller
    {
        private readonly HttpClient _api;

        protected ControladorConSesion(IHttpClientFactory httpClientFactory)
        {
            _api = httpClientFactory.CreateClient("MealPrepAPI");
        }

        protected IActionResult? ValidarAcceso(int rolRequerido)
        {
            if (HttpContext.Session.GetInt32("Autenticado") != 1)
            {
                return RedirectToAction("Login", "Cuenta");
            }

            if (HttpContext.Session.GetInt32("IdRol") != rolRequerido)
            {
                return StatusCode(403);
            }

            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("TokenJWT")))
            {
                return CerrarPorVencimiento();
            }

            return null;
        }

        protected async Task<HttpResponseMessage> EnviarAsync(
            HttpMethod metodo,
            string ruta,
            object? contenido = null)
        {
            using var solicitud = new HttpRequestMessage(metodo, ruta);

            solicitud.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                HttpContext.Session.GetString("TokenJWT"));

            if (contenido != null)
            {
                solicitud.Content = JsonContent.Create(contenido);
            }

            return await _api.SendAsync(solicitud);
        }

        protected static bool EsSesionVencida(HttpResponseMessage respuesta)
        {
            return respuesta.StatusCode == HttpStatusCode.Unauthorized
                || respuesta.StatusCode == HttpStatusCode.Forbidden;
        }

        protected IActionResult CerrarPorVencimiento()
        {
            HttpContext.Session.Clear();
            TempData["Error"] = "Tu sesión ha vencido. Inicia sesión nuevamente.";
            return RedirectToAction("Login", "Cuenta");
        }

        protected static async Task<string?> LeerMensajeAsync(HttpResponseMessage respuesta)
        {
            try
            {
                var datos = await respuesta.Content.ReadFromJsonAsync<ApiMensaje>();

                return string.IsNullOrWhiteSpace(datos?.Mensaje) ? null : datos.Mensaje;
            }
            catch (Exception ex) when (ex is JsonException || ex is NotSupportedException)
            {
                return null;
            }
        }
    }
}
