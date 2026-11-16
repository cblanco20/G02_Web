using Dapper;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;

namespace G02_API.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]")]
    public class ErrorController(
        IConfiguration configuration,
        ILogger<ErrorController> logger) : ControllerBase
    {
        [Route("CapturarError")]
        public IActionResult CapturarError()
        {
            var excepcion = HttpContext.Features.Get<IExceptionHandlerFeature>();

            if (excepcion == null)
            {
                return NotFound();
            }

            logger.LogError(excepcion.Error, "Error no controlado en {Origen}", excepcion.Path);

            try
            {
                int? idUsuario = int.TryParse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier),
                    out int id) ? id : null;

                using var conexion = new SqlConnection(
                    configuration.GetValue<string>("Variables:ConnectionString"));

                var parametros = new DynamicParameters();
                parametros.Add("@IdUsuario", idUsuario, DbType.Int32);
                parametros.Add("@Mensaje", excepcion.Error.ToString());
                parametros.Add("@Origen", excepcion.Path);

                conexion.Execute(
                    "dbo.sp_RegistrarError",
                    parametros,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "No se pudo registrar el error en la base de datos");
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { mensaje = "Ocurrió un error técnico. Inténtelo nuevamente." });
        }
    }
}