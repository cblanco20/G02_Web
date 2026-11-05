using Dapper;
using G02_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace G02_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<RegistroUsuario> _passwordHasher;

        public UsuariosController(IConfiguration configuration)
        {
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<RegistroUsuario>();
        }

        [HttpPost("Registrar")]
        public IActionResult Registrar([FromBody] RegistroUsuario modelo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var connectionString = _configuration
                    .GetValue<string>("Variables:ConnectionString");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return StatusCode(500, new
                    {
                        mensaje = "La conexión no está configurada."
                    });
                }

                string contrasenaHash = _passwordHasher.HashPassword(
                    modelo,
                    modelo.Contrasena
                );

                using var conexion = new SqlConnection(connectionString);

                var parametros = new DynamicParameters();

                parametros.Add("@NombreUsuario", modelo.NombreUsuario);
                parametros.Add("@Correo", modelo.Correo);
                parametros.Add("@ContrasenaHash", contrasenaHash);

                int idUsuario = conexion.QuerySingle<int>(
                    "dbo.sp_RegistrarUsuario",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(new
                {
                    mensaje = "Usuario registrado correctamente.",
                    idUsuario
                });
            }
            catch (SqlException ex) when (ex.Number == 50002 ||
                                          ex.Number == 2601 ||
                                          ex.Number == 2627)
            {
                return Conflict(new
                {
                    mensaje = "El correo ya se encuentra registrado."
                });
            }
            catch (SqlException ex) when (ex.Number == 50001)
            {
                return BadRequest(new
                {
                    mensaje = "Debe completar los campos obligatorios."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "Ocurrió un error al registrar el usuario."
                });
            }
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequestModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var connectionString = _configuration
                    .GetValue<string>("Variables:ConnectionString");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return StatusCode(500, new
                    {
                        mensaje = "La conexión no está configurada."
                    });
                }

                using var conexion = new SqlConnection(connectionString);

                var parametros = new DynamicParameters();
                parametros.Add("@Correo", modelo.Correo);

                var usuario = conexion.QueryFirstOrDefault<LoginResponseModel>(
                    "dbo.sp_ConsultarUsuarioLogin",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                if (usuario == null || !usuario.Activo)
                {
                    return Unauthorized(new
                    {
                        mensaje = "Correo o contraseña incorrectos."
                    });
                }

                var resultado = _passwordHasher.VerifyHashedPassword(
                    new RegistroUsuario(),
                    usuario.ContrasenaHash,
                    modelo.Contrasena
                );

                if (resultado == PasswordVerificationResult.Failed)
                {
                    return Unauthorized(new
                    {
                        mensaje = "Correo o contraseña incorrectos."
                    });
                }

                return Ok(new
                {
                    mensaje = "Inicio de sesión correcto.",
                    usuario.IdUsuario,
                    usuario.NombreUsuario,
                    usuario.Correo,
                    usuario.IdRol,
                    usuario.NombreRol
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "Ocurrió un error al iniciar sesión."
                });
            }
        }

        [HttpGet("Consultar")]
        public IActionResult Consultar()
        {
            try
            {
                var connectionString = _configuration
                    .GetValue<string>("Variables:ConnectionString");

                using var conexion = new SqlConnection(connectionString);

                var usuarios = conexion.Query<UsuarioConsulta>(
                    "dbo.sp_ConsultarUsuarios",
                    commandType: CommandType.StoredProcedure
                ).ToList();

                return Ok(usuarios);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    Mensaje = "Ocurrió un error al consultar los usuarios."
                });
            }
        }
        [HttpPut("CambiarEstado")]
        public IActionResult CambiarEstado([FromBody] CambiarEstadoUsuario modelo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var connectionString = _configuration
                    .GetValue<string>("Variables:ConnectionString");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return StatusCode(500, new
                    {
                        mensaje = "La conexión no está configurada."
                    });
                }

                using var conexion = new SqlConnection(connectionString);

                var parametros = new DynamicParameters();

                parametros.Add("@IdUsuario", modelo.IdUsuario);
                parametros.Add("@Activo", modelo.Activo);

                conexion.Execute(
                    "dbo.sp_CambiarEstadoUsuario",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(new
                {
                    mensaje = modelo.Activo == true
                        ? "Usuario activado correctamente."
                        : "Usuario desactivado correctamente."
                });
            }
            catch (SqlException ex) when (ex.Number == 50003)
            {
                return NotFound(new
                {
                    mensaje = "El usuario no existe."
                });
            }
            catch (SqlException ex) when (ex.Number == 50004)
            {
                return BadRequest(new
                {
                    mensaje = "No se permite modificar el estado de una cuenta administradora."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensaje = "Ocurrió un error al cambiar el estado del usuario."
                });
            }
        }
    }
}