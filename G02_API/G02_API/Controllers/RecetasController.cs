using Dapper;
using G02_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;
using System.Text.Json;

namespace G02_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RecetasController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public RecetasController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("Categorias")]
        public IActionResult Categorias()
        {
            using var conexion = CrearConexion();

            var categorias = conexion.Query<CategoriaConsulta>(
                "dbo.sp_ConsultarCategorias",
                commandType: CommandType.StoredProcedure
            ).ToList();

            return Ok(categorias);
        }

        [HttpGet("Consultar")]
        public IActionResult Consultar([FromQuery] int? idCategoria, [FromQuery] string? busqueda)
        {
            int? idUsuario = ObtenerIdUsuarioAutenticado();

            if (idUsuario == null)
            {
                return Unauthorized();
            }

            using var conexion = CrearConexion();

            var parametros = new DynamicParameters();
            parametros.Add("@IdUsuario", idUsuario.Value, DbType.Int32);
            parametros.Add("@IdCategoria", idCategoria, DbType.Int32);
            parametros.Add(
                "@Busqueda",
                string.IsNullOrWhiteSpace(busqueda) ? null : busqueda.Trim(),
                DbType.String);

            var recetas = conexion.Query<RecetaResumen>(
                "dbo.sp_ConsultarRecetas",
                parametros,
                commandType: CommandType.StoredProcedure
            ).ToList();

            return Ok(recetas);
        }

        [HttpGet("Consultar/{idReceta:int}")]
        public IActionResult ConsultarDetalle(int idReceta)
        {
            int? idUsuario = ObtenerIdUsuarioAutenticado();

            if (idUsuario == null)
            {
                return Unauthorized();
            }

            using var conexion = CrearConexion();

            var parametros = new DynamicParameters();
            parametros.Add("@IdReceta", idReceta, DbType.Int32);
            parametros.Add("@IdUsuario", idUsuario.Value, DbType.Int32);

            using var resultado = conexion.QueryMultiple(
                "dbo.sp_ConsultarReceta",
                parametros,
                commandType: CommandType.StoredProcedure);

            var receta = resultado.ReadFirstOrDefault<RecetaDetalle>();

            if (receta == null)
            {
                return NotFound(new { mensaje = "La receta no existe." });
            }

            receta.Ingredientes = resultado.Read<RecetaIngredienteDetalle>().ToList();

            return Ok(receta);
        }

        [HttpPost("Registrar")]
        public IActionResult Registrar([FromBody] RecetaRequest modelo)
        {
            int? idUsuario = ObtenerIdUsuarioAutenticado();

            if (idUsuario == null)
            {
                return Unauthorized();
            }

            try
            {
                using var conexion = CrearConexion();

                var parametros = CrearParametros(modelo, idUsuario.Value, null);

                int idReceta = conexion.QuerySingle<int>(
                    "dbo.sp_RegistrarReceta",
                    parametros,
                    commandType: CommandType.StoredProcedure);

                return Ok(new
                {
                    mensaje = "Receta registrada correctamente.",
                    idReceta
                });
            }
            catch (SqlException ex) when (ex.Number >= 50020 && ex.Number <= 50027)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("Actualizar/{idReceta:int}")]
        public IActionResult Actualizar(int idReceta, [FromBody] RecetaRequest modelo)
        {
            int? idUsuario = ObtenerIdUsuarioAutenticado();

            if (idUsuario == null)
            {
                return Unauthorized();
            }

            try
            {
                using var conexion = CrearConexion();

                var parametros = CrearParametros(modelo, idUsuario.Value, idReceta);

                conexion.Execute(
                    "dbo.sp_ActualizarReceta",
                    parametros,
                    commandType: CommandType.StoredProcedure);

                return Ok(new { mensaje = "Receta actualizada correctamente." });
            }
            catch (SqlException ex) when (ex.Number == 50028)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (SqlException ex) when (ex.Number >= 50020 && ex.Number <= 50027)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("Eliminar/{idReceta:int}")]
        public IActionResult Eliminar(int idReceta)
        {
            int? idUsuario = ObtenerIdUsuarioAutenticado();

            if (idUsuario == null)
            {
                return Unauthorized();
            }

            try
            {
                using var conexion = CrearConexion();

                var parametros = new DynamicParameters();
                parametros.Add("@IdReceta", idReceta, DbType.Int32);
                parametros.Add("@IdUsuario", idUsuario.Value, DbType.Int32);

                conexion.Execute(
                    "dbo.sp_EliminarReceta",
                    parametros,
                    commandType: CommandType.StoredProcedure);

                return Ok(new { mensaje = "Receta eliminada correctamente." });
            }
            catch (SqlException ex) when (ex.Number == 50028)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        private static DynamicParameters CrearParametros(
            RecetaRequest modelo,
            int idUsuario,
            int? idReceta)
        {
            var ingredientes = modelo.Ingredientes.Select(i => new
            {
                idIngrediente = i.IdIngrediente,
                cantidad = i.Cantidad
            });

            var parametros = new DynamicParameters();

            if (idReceta.HasValue)
            {
                parametros.Add("@IdReceta", idReceta.Value, DbType.Int32);
            }

            parametros.Add("@IdUsuario", idUsuario, DbType.Int32);
            parametros.Add("@IdCategoria", modelo.IdCategoria, DbType.Int32);
            parametros.Add("@NombreReceta", modelo.NombreReceta.Trim(), DbType.String);
            parametros.Add(
                "@Descripcion",
                string.IsNullOrWhiteSpace(modelo.Descripcion) ? null : modelo.Descripcion.Trim(),
                DbType.String);
            parametros.Add("@Instrucciones", modelo.Instrucciones.Trim(), DbType.String, size: -1);
            parametros.Add("@TiempoPreparacion", modelo.TiempoPreparacion, DbType.Int32);
            parametros.Add("@Porciones", modelo.Porciones, DbType.Int32);
            parametros.Add(
                "@Ingredientes",
                JsonSerializer.Serialize(ingredientes),
                DbType.String,
                size: -1);

            return parametros;
        }

        private SqlConnection CrearConexion()
        {
            return new SqlConnection(
                _configuration.GetValue<string>("Variables:ConnectionString"));
        }

        private int? ObtenerIdUsuarioAutenticado()
        {
            string? identificador = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(identificador, out int idUsuario) && idUsuario > 0)
            {
                return idUsuario;
            }

            return null;
        }
    }
}
