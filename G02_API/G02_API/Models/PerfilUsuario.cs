namespace G02_API.Models
{
    public class PerfilUsuario
    {
        public int IdUsuario { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public int IdRol { get; set; }

        public string NombreRol { get; set; } = string.Empty;
    }
}
