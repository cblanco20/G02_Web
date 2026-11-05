namespace G02_PrograAvanzadaWeb.Models
{
    public class UsuarioConsulta
    {
        public int IdUsuario { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public int IdRol { get; set; }

        public string NombreRol { get; set; } = string.Empty;

        public bool Activo { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}