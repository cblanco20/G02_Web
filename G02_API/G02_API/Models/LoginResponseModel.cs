namespace G02_API.Models
{
    public class LoginResponseModel
    {
        public int IdUsuario { get; set; }

        public int IdRol { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string ContrasenaHash { get; set; } = string.Empty;

        public bool Activo { get; set; }

        public string NombreRol { get; set; } = string.Empty;
    }
}
