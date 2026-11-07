using System.ComponentModel.DataAnnotations;

namespace G02_PrograAvanzadaWeb.Models
{
    public class PerfilUsuario
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(150)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        public int IdRol { get; set; }

        public string NombreRol { get; set; } = string.Empty;
    }
}
