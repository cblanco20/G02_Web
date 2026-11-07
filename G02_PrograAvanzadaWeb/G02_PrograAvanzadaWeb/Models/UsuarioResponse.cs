namespace G02_PrograAvanzadaWeb.Models
{
    public class UsuarioResponse
    {
        public string Mensaje { get; set; } = string.Empty;

        public int IdUsuario { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public int IdRol { get; set; }

        public string NombreRol { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}