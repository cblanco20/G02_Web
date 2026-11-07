using System.ComponentModel.DataAnnotations;

namespace G02_API.Models
{
    public class RestablecerContrasenaRequest
    {
        [Required(ErrorMessage = "El token de recuperación es obligatorio.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string NuevaContrasena { get; set; } = string.Empty;
    }
}
