using G02_PrograAvanzadaWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;

namespace G02_PrograAvanzadaWeb.Controllers
{
    public class RecetasController : ControladorConSesion
    {
        public RecetasController(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? idCategoria, string? busqueda)
        {
            var acceso = ValidarAcceso(1);
            if (acceso != null)
            {
                return acceso;
            }

            var modelo = new RecetasIndexViewModel
            {
                IdCategoria = idCategoria,
                Busqueda = busqueda
            };

            try
            {
                var filtros = new List<string>();

                if (idCategoria.HasValue && idCategoria.Value > 0)
                {
                    filtros.Add("idCategoria=" + idCategoria.Value);
                }

                if (!string.IsNullOrWhiteSpace(busqueda))
                {
                    filtros.Add("busqueda=" + Uri.EscapeDataString(busqueda.Trim()));
                }

                string ruta = "api/Recetas/Consultar";

                if (filtros.Count > 0)
                {
                    ruta += "?" + string.Join("&", filtros);
                }

                using var respuestaRecetas = await EnviarAsync(HttpMethod.Get, ruta);

                if (EsSesionVencida(respuestaRecetas))
                {
                    return CerrarPorVencimiento();
                }

                if (respuestaRecetas.IsSuccessStatusCode)
                {
                    modelo.Recetas = await respuestaRecetas.Content
                        .ReadFromJsonAsync<List<RecetaResumen>>()
                        ?? new List<RecetaResumen>();
                }
                else
                {
                    TempData["Error"] = "No se pudieron consultar las recetas.";
                }

                using var respuestaCategorias = await EnviarAsync(
                    HttpMethod.Get,
                    "api/Recetas/Categorias");

                if (respuestaCategorias.IsSuccessStatusCode)
                {
                    modelo.Categorias = await respuestaCategorias.Content
                        .ReadFromJsonAsync<List<CategoriaItem>>()
                        ?? new List<CategoriaItem>();
                }
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Ocurrió un error al conectar con la API.";
            }

            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var acceso = ValidarAcceso(1);
            if (acceso != null)
            {
                return acceso;
            }

            var (detalle, vencida) = await ObtenerDetalleAsync(id);

            if (vencida)
            {
                return CerrarPorVencimiento();
            }

            if (detalle == null)
            {
                TempData["Error"] = "La receta no existe o no se pudo consultar.";
                return RedirectToAction(nameof(Index));
            }

            return View(detalle);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var acceso = ValidarAcceso(1);
            if (acceso != null)
            {
                return acceso;
            }

            var modelo = new RecetaFormulario
            {
                TiempoPreparacion = 30,
                Porciones = 2
            };

            var redireccion = await CargarCatalogosAsync(modelo, null);
            if (redireccion != null)
            {
                return redireccion;
            }

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(RecetaFormulario modelo)
        {
            var acceso = ValidarAcceso(1);
            if (acceso != null)
            {
                return acceso;
            }

            var solicitud = ArmarSolicitud(modelo);

            if (ModelState.IsValid && solicitud != null)
            {
                try
                {
                    using var respuesta = await EnviarAsync(
                        HttpMethod.Post,
                        "api/Recetas/Registrar",
                        solicitud);

                    if (EsSesionVencida(respuesta))
                    {
                        return CerrarPorVencimiento();
                    }

                    if (respuesta.IsSuccessStatusCode)
                    {
                        TempData["Exito"] = "La receta se registró correctamente.";
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError(
                        string.Empty,
                        await LeerMensajeAsync(respuesta) ?? "No se pudo registrar la receta.");
                }
                catch (HttpRequestException)
                {
                    ModelState.AddModelError(string.Empty, "Ocurrió un error al conectar con la API.");
                }
            }

            var redireccion = await CargarCatalogosAsync(modelo, null);
            if (redireccion != null)
            {
                return redireccion;
            }

            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var acceso = ValidarAcceso(1);
            if (acceso != null)
            {
                return acceso;
            }

            var (detalle, vencida) = await ObtenerDetalleAsync(id);

            if (vencida)
            {
                return CerrarPorVencimiento();
            }

            if (detalle == null)
            {
                TempData["Error"] = "La receta no existe o no se pudo consultar.";
                return RedirectToAction(nameof(Index));
            }

            var modelo = new RecetaFormulario
            {
                IdReceta = detalle.IdReceta,
                NombreReceta = detalle.NombreReceta,
                Descripcion = detalle.Descripcion,
                IdCategoria = detalle.IdCategoria,
                TiempoPreparacion = detalle.TiempoPreparacion,
                Porciones = detalle.Porciones,
                Instrucciones = detalle.Instrucciones,
                Ingredientes = detalle.Ingredientes
                    .Select(i => new RecetaIngredienteFormulario
                    {
                        IdIngrediente = i.IdIngrediente,
                        Cantidad = i.Cantidad.ToString("0.##", CultureInfo.InvariantCulture)
                    })
                    .ToList()
            };

            var redireccion = await CargarCatalogosAsync(modelo, detalle.Ingredientes);
            if (redireccion != null)
            {
                return redireccion;
            }

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, RecetaFormulario modelo)
        {
            var acceso = ValidarAcceso(1);
            if (acceso != null)
            {
                return acceso;
            }

            modelo.IdReceta = id;

            var solicitud = ArmarSolicitud(modelo);

            if (ModelState.IsValid && solicitud != null)
            {
                try
                {
                    using var respuesta = await EnviarAsync(
                        HttpMethod.Put,
                        "api/Recetas/Actualizar/" + id,
                        solicitud);

                    if (EsSesionVencida(respuesta))
                    {
                        return CerrarPorVencimiento();
                    }

                    if (respuesta.IsSuccessStatusCode)
                    {
                        TempData["Exito"] = "La receta se actualizó correctamente.";
                        return RedirectToAction(nameof(Index));
                    }

                    if (respuesta.StatusCode == HttpStatusCode.NotFound)
                    {
                        TempData["Error"] = "La receta no existe.";
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError(
                        string.Empty,
                        await LeerMensajeAsync(respuesta) ?? "No se pudo actualizar la receta.");
                }
                catch (HttpRequestException)
                {
                    ModelState.AddModelError(string.Empty, "Ocurrió un error al conectar con la API.");
                }
            }

            var (detalle, vencida) = await ObtenerDetalleAsync(id);

            if (vencida)
            {
                return CerrarPorVencimiento();
            }

            var redireccion = await CargarCatalogosAsync(modelo, detalle?.Ingredientes);
            if (redireccion != null)
            {
                return redireccion;
            }

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var acceso = ValidarAcceso(1);
            if (acceso != null)
            {
                return acceso;
            }

            try
            {
                using var respuesta = await EnviarAsync(
                    HttpMethod.Delete,
                    "api/Recetas/Eliminar/" + id);

                if (EsSesionVencida(respuesta))
                {
                    return CerrarPorVencimiento();
                }

                if (respuesta.IsSuccessStatusCode)
                {
                    TempData["Exito"] = "La receta se eliminó correctamente.";
                }
                else
                {
                    TempData["Error"] = await LeerMensajeAsync(respuesta)
                        ?? "No se pudo eliminar la receta.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Ocurrió un error al conectar con la API.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<(RecetaDetalleItem? Detalle, bool SesionVencida)> ObtenerDetalleAsync(int id)
        {
            try
            {
                using var respuesta = await EnviarAsync(
                    HttpMethod.Get,
                    "api/Recetas/Consultar/" + id);

                if (EsSesionVencida(respuesta))
                {
                    return (null, true);
                }

                if (!respuesta.IsSuccessStatusCode)
                {
                    return (null, false);
                }

                var detalle = await respuesta.Content.ReadFromJsonAsync<RecetaDetalleItem>();

                return (detalle, false);
            }
            catch (HttpRequestException)
            {
                return (null, false);
            }
        }

        private async Task<IActionResult?> CargarCatalogosAsync(
            RecetaFormulario modelo,
            List<RecetaIngredienteItem>? existentes)
        {
            try
            {
                using var respuestaCategorias = await EnviarAsync(
                    HttpMethod.Get,
                    "api/Recetas/Categorias");

                if (EsSesionVencida(respuestaCategorias))
                {
                    return CerrarPorVencimiento();
                }

                if (respuestaCategorias.IsSuccessStatusCode)
                {
                    modelo.Categorias = await respuestaCategorias.Content
                        .ReadFromJsonAsync<List<CategoriaItem>>()
                        ?? new List<CategoriaItem>();
                }

                using var respuestaIngredientes = await EnviarAsync(
                    HttpMethod.Get,
                    "api/Ingredientes/Consultar");

                if (EsSesionVencida(respuestaIngredientes))
                {
                    return CerrarPorVencimiento();
                }

                if (respuestaIngredientes.IsSuccessStatusCode)
                {
                    modelo.IngredientesDisponibles = await respuestaIngredientes.Content
                        .ReadFromJsonAsync<List<IngredienteItem>>()
                        ?? new List<IngredienteItem>();
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No se pudieron cargar las categorías e ingredientes desde la API.");
            }

            if (existentes != null)
            {
                foreach (var existente in existentes)
                {
                    if (modelo.IngredientesDisponibles.All(i => i.IdIngrediente != existente.IdIngrediente))
                    {
                        modelo.IngredientesDisponibles.Add(new IngredienteItem
                        {
                            IdIngrediente = existente.IdIngrediente,
                            NombreIngrediente = existente.NombreIngrediente + " (inactivo)",
                            NombreUnidad = existente.NombreUnidad,
                            Abreviatura = existente.Abreviatura,
                            Activo = false
                        });
                    }
                }
            }

            modelo.IngredientesDisponibles = modelo.IngredientesDisponibles
                .OrderBy(i => i.NombreIngrediente)
                .ToList();

            return null;
        }

        private object? ArmarSolicitud(RecetaFormulario modelo)
        {
            var ingredientes = new List<object>();
            var usados = new HashSet<int>();
            var errores = new List<string>();

            foreach (var fila in modelo.Ingredientes)
            {
                bool sinIngrediente = fila.IdIngrediente <= 0;
                bool sinCantidad = string.IsNullOrWhiteSpace(fila.Cantidad);

                if (sinIngrediente && sinCantidad)
                {
                    continue;
                }

                if (sinIngrediente)
                {
                    AgregarError(errores, "Seleccione el ingrediente en cada fila que tenga cantidad.");
                    continue;
                }

                bool cantidadValida = decimal.TryParse(
                    fila.Cantidad?.Trim().Replace(',', '.'),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal cantidad);

                if (!cantidadValida || cantidad <= 0)
                {
                    AgregarError(errores, "Ingrese una cantidad mayor que cero en cada ingrediente.");
                    continue;
                }

                if (!usados.Add(fila.IdIngrediente))
                {
                    AgregarError(errores, "No repita ingredientes en la receta.");
                    continue;
                }

                ingredientes.Add(new
                {
                    IdIngrediente = fila.IdIngrediente,
                    Cantidad = cantidad
                });
            }

            if (errores.Count == 0 && ingredientes.Count == 0)
            {
                AgregarError(errores, "Agregue al menos un ingrediente a la receta.");
            }

            foreach (var error in errores)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            if (errores.Count > 0)
            {
                return null;
            }

            return new
            {
                IdCategoria = modelo.IdCategoria,
                NombreReceta = modelo.NombreReceta?.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(modelo.Descripcion)
                    ? null
                    : modelo.Descripcion.Trim(),
                Instrucciones = modelo.Instrucciones?.Trim(),
                TiempoPreparacion = modelo.TiempoPreparacion,
                Porciones = modelo.Porciones,
                Ingredientes = ingredientes
            };
        }

        private static void AgregarError(List<string> errores, string mensaje)
        {
            if (!errores.Contains(mensaje))
            {
                errores.Add(mensaje);
            }
        }
    }
}
