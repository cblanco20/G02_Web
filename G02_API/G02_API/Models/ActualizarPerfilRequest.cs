using System.ComponentModel.DataAnnotations;

namespace G02_API.Models
{
    public class ActualizarPerfilRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(150)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;
    }
}
