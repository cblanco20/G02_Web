using Dapper;
using G02_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Data;

namespace G02_API.Services
{
    public class InicializadorAdministrador
    {
        public static void Inicializar(IConfiguration configuration)
        {
            var connectionString = configuration["Variables:ConnectionString"];
            var nombre = configuration["AdministradorInicial:NombreUsuario"];
            var correo = configuration["AdministradorInicial:Correo"];
            var contrasena = configuration["AdministradorInicial:Contrasena"];

            // No inicializar si faltan datos de configuración.
            if (string.IsNullOrWhiteSpace(connectionString) ||
                string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(contrasena))
            {
                throw new InvalidOperationException(
                    "Falta la configuración del administrador inicial.");
            }

            using var conexion = new SqlConnection(connectionString);

            // Revisar si ya existe un administrador.
            int cantidadAdministradores = conexion.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM dbo.TB_Usuarios WHERE IdRol = 2"
            );

            if (cantidadAdministradores > 0)
            {
                return;
            }

            // Generar el hash con el mismo sistema utilizado en el login.
            var passwordHasher = new PasswordHasher<RegistroUsuario>();
            var modelo = new RegistroUsuario();

            string contrasenaHash = passwordHasher.HashPassword(
                modelo,
                contrasena
            );

            var parametros = new DynamicParameters();
            parametros.Add("@NombreUsuario", nombre);
            parametros.Add("@Correo", correo);
            parametros.Add("@ContrasenaHash", contrasenaHash);

            // Crear el administrador mediante el procedimiento almacenado.
            conexion.Execute(
                "dbo.sp_CrearAdministradorInicial",
                parametros,
                commandType: CommandType.StoredProcedure
            );
        }

    }
}
