using System.ComponentModel.DataAnnotations;

namespace G02_PrograAvanzadaWeb.Models
{
    public class RegistroUsuario
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [StringLength(150)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string Contrasena { get; set; } = string.Empty;

        [Required]
        public bool AceptaTerminos { get; set; }
    }
}