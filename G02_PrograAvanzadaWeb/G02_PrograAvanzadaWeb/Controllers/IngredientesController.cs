using G02_PrograAvanzadaWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

namespace G02_PrograAvanzadaWeb.Controllers
{
    public class IngredientesController : ControladorConSesion
    {
        public IngredientesController(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso(2);
            if (acceso != null)
            {
                return acceso;
            }

            var modelo = new IngredientesIndexViewModel();

            try
            {
                using var respuestaIngredientes = await EnviarAsync(
                    HttpMethod.Get,
                    "api/Ingredientes/Consultar?soloActivos=false");

                if (EsSesionVencida(respuestaIngredientes))
                {
                    return CerrarPorVencimiento();
                }

                if (respuestaIngredientes.IsSuccessStatusCode)
                {
                    modelo.Ingredientes = await respuestaIngredientes.Content
                        .ReadFromJsonAsync<List<IngredienteItem>>()
                        ?? new List<IngredienteItem>();
                }
                else
                {
                    TempData["Error"] = "No se pudieron consultar los ingredientes.";
                }

                using var respuestaUnidades = await EnviarAsync(
                    HttpMethod.Get,
                    "api/Ingredientes/Unidades");

                if (respuestaUnidades.IsSuccessStatusCode)
                {
                    modelo.Unidades = await respuestaUnidades.Content
                        .ReadFromJsonAsync<List<UnidadItem>>()
                        ?? new List<UnidadItem>();
                }
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Ocurrió un error al conectar con la API.";
            }

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(string? nombreIngrediente, int idUnidad)
        {
            var acceso = ValidarAcceso(2);
            if (acceso != null)
            {
                return acceso;
            }

            if (string.IsNullOrWhiteSpace(nombreIngrediente) || idUnidad <= 0)
            {
                TempData["Error"] = "Ingrese el nombre del ingrediente y seleccione su unidad.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                using var respuesta = await EnviarAsync(
                    HttpMethod.Post,
                    "api/Ingredientes/Registrar",
                    new
                    {
                        NombreIngrediente = nombreIngrediente.Trim(),
                        IdUnidad = idUnidad
                    });

                if (EsSesionVencida(respuesta))
                {
                    return CerrarPorVencimiento();
                }

                if (respuesta.IsSuccessStatusCode)
                {
                    TempData["Exito"] = "El ingrediente se registró correctamente.";
                }
                else
                {
                    TempData["Error"] = await LeerMensajeAsync(respuesta)
                        ?? "No se pudo registrar el ingrediente.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Ocurrió un error al conectar con la API.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var acceso = ValidarAcceso(2);
            if (acceso != null)
            {
                return acceso;
            }

            try
            {
                using var respuesta = await EnviarAsync(
                    HttpMethod.Get,
                    "api/Ingredientes/Consultar?soloActivos=false");

                if (EsSesionVencida(respuesta))
                {
                    return CerrarPorVencimiento();
                }

                if (respuesta.IsSuccessStatusCode)
                {
                    var ingredientes = await respuesta.Content
                        .ReadFromJsonAsync<List<IngredienteItem>>()
                        ?? new List<IngredienteItem>();

                    var ingrediente = ingredientes.FirstOrDefault(i => i.IdIngrediente == id);

                    if (ingrediente != null)
                    {
                        var modelo = new IngredienteFormulario
                        {
                            IdIngrediente = ingrediente.IdIngrediente,
                            NombreIngrediente = ingrediente.NombreIngrediente,
                            IdUnidad = ingrediente.IdUnidad
                        };

                        var redireccion = await CargarUnidadesAsync(modelo);
                        if (redireccion != null)
                        {
                            return redireccion;
                        }

                        return View(modelo);
                    }
                }
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Ocurrió un error al conectar con la API.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "El ingrediente no existe.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, IngredienteFormulario modelo)
        {
            var acceso = ValidarAcceso(2);
            if (acceso != null)
            {
                return acceso;
            }

            modelo.IdIngrediente = id;

            if (ModelState.IsValid)
            {
                try
                {
                    using var respuesta = await EnviarAsync(
                        HttpMethod.Put,
                        "api/Ingredientes/Actualizar/" + id,
                        new
                        {
                            NombreIngrediente = modelo.NombreIngrediente.Trim(),
                            IdUnidad = modelo.IdUnidad
                        });

                    if (EsSesionVencida(respuesta))
                    {
                        return CerrarPorVencimiento();
                    }

                    if (respuesta.IsSuccessStatusCode)
                    {
                        TempData["Exito"] = "El ingrediente se actualizó correctamente.";
                        return RedirectToAction(nameof(Index));
                    }

                    if (respuesta.StatusCode == HttpStatusCode.NotFound)
                    {
                        TempData["Error"] = "El ingrediente no existe.";
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError(
                        string.Empty,
                        await LeerMensajeAsync(respuesta) ?? "No se pudo actualizar el ingrediente.");
                }
                catch (HttpRequestException)
                {
                    ModelState.AddModelError(string.Empty, "Ocurrió un error al conectar con la API.");
                }
            }

            var redireccion = await CargarUnidadesAsync(modelo);
            if (redireccion != null)
            {
                return redireccion;
            }

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int idIngrediente, bool activo)
        {
            var acceso = ValidarAcceso(2);
            if (acceso != null)
            {
                return acceso;
            }

            try
            {
                using var respuesta = await EnviarAsync(
                    HttpMethod.Put,
                    "api/Ingredientes/CambiarEstado",
                    new
                    {
                        IdIngrediente = idIngrediente,
                        Activo = activo
                    });

                if (EsSesionVencida(respuesta))
                {
                    return CerrarPorVencimiento();
                }

                if (respuesta.IsSuccessStatusCode)
                {
                    TempData["Exito"] = activo
                        ? "El ingrediente fue activado correctamente."
                        : "El ingrediente fue desactivado correctamente.";
                }
                else
                {
                    TempData["Error"] = await LeerMensajeAsync(respuesta)
                        ?? "No se pudo cambiar el estado del ingrediente.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Ocurrió un error al conectar con la API.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult?> CargarUnidadesAsync(IngredienteFormulario modelo)
        {
            try
            {
                using var respuesta = await EnviarAsync(
                    HttpMethod.Get,
                    "api/Ingredientes/Unidades");

                if (EsSesionVencida(respuesta))
                {
                    return CerrarPorVencimiento();
                }

                if (respuesta.IsSuccessStatusCode)
                {
                    modelo.Unidades = await respuesta.Content
                        .ReadFromJsonAsync<List<UnidadItem>>()
                        ?? new List<UnidadItem>();
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "No se pudieron cargar las unidades de medida.");
            }

            return null;
        }
    }
}
