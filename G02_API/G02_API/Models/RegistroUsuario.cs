using System.ComponentModel.DataAnnotations;

namespace G02_API.Models
{
    public class RegistroUsuario
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string Contrasena { get; set; } = string.Empty;
    }
}
