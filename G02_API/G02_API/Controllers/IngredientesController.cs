using Dapper;
using G02_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace G02_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class IngredientesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public IngredientesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("Unidades")]
        public IActionResult Unidades()
        {
            using var conexion = CrearConexion();

            var unidades = conexion.Query<UnidadConsulta>(
                "dbo.sp_ConsultarUnidades",
                commandType: CommandType.StoredProcedure
            ).ToList();

            return Ok(unidades);
        }

        [HttpGet("Consultar")]
        public IActionResult Consultar([FromQuery] bool soloActivos = true)
        {
            if (!soloActivos && !User.IsInRole("2"))
            {
                return Forbid();
            }

            using var conexion = CrearConexion();

            var parametros = new DynamicParameters();
            parametros.Add("@SoloActivos", soloActivos, DbType.Boolean);

            var ingredientes = conexion.Query<IngredienteConsulta>(
                "dbo.sp_ConsultarIngredientes",
                parametros,
                commandType: CommandType.StoredProcedure
            ).ToList();

            return Ok(ingredientes);
        }

        [Authorize(Roles = "2")]
        [HttpPost("Registrar")]
        public IActionResult Registrar([FromBody] IngredienteRequest modelo)
        {
            try
            {
                using var conexion = CrearConexion();

                var parametros = new DynamicParameters();
                parametros.Add("@NombreIngrediente", modelo.NombreIngrediente.Trim(), DbType.String);
                parametros.Add("@IdUnidad", modelo.IdUnidad, DbType.Int32);

                int idIngrediente = conexion.QuerySingle<int>(
                    "dbo.sp_RegistrarIngrediente",
                    parametros,
                    commandType: CommandType.StoredProcedure);

                return Ok(new
                {
                    mensaje = "Ingrediente registrado correctamente.",
                    idIngrediente
                });
            }
            catch (SqlException ex) when (ex.Number >= 50010 && ex.Number <= 50014)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [Authorize(Roles = "2")]
        [HttpPut("Actualizar/{idIngrediente:int}")]
        public IActionResult Actualizar(int idIngrediente, [FromBody] IngredienteRequest modelo)
        {
            try
            {
                using var conexion = CrearConexion();

                var parametros = new DynamicParameters();
                parametros.Add("@IdIngrediente", idIngrediente, DbType.Int32);
                parametros.Add("@NombreIngrediente", modelo.NombreIngrediente.Trim(), DbType.String);
                parametros.Add("@IdUnidad", modelo.IdUnidad, DbType.Int32);

                conexion.Execute(
                    "dbo.sp_ActualizarIngrediente",
                    parametros,
                    commandType: CommandType.StoredProcedure);

                return Ok(new { mensaje = "Ingrediente actualizado correctamente." });
            }
            catch (SqlException ex) when (ex.Number == 50013)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (SqlException ex) when (ex.Number >= 50010 && ex.Number <= 50014)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [Authorize(Roles = "2")]
        [HttpPut("CambiarEstado")]
        public IActionResult CambiarEstado([FromBody] CambiarEstadoIngrediente modelo)
        {
            try
            {
                using var conexion = CrearConexion();

                var parametros = new DynamicParameters();
                parametros.Add("@IdIngrediente", modelo.IdIngrediente, DbType.Int32);
                parametros.Add("@Activo", modelo.Activo!.Value, DbType.Boolean);

                conexion.Execute(
                    "dbo.sp_CambiarEstadoIngrediente",
                    parametros,
                    commandType: CommandType.StoredProcedure);

                return Ok(new { mensaje = "Estado actualizado correctamente." });
            }
            catch (SqlException ex) when (ex.Number == 50013)
            {
                return NotFound(new { mensaje = ex.Message });
            }
        }

        private SqlConnection CrearConexion()
        {
            return new SqlConnection(
                _configuration.GetValue<string>("Variables:ConnectionString"));
        }
    }
}
