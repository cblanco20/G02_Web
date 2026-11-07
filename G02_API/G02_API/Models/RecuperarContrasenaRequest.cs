using System.ComponentModel.DataAnnotations;

namespace G02_API.Models
{
    public class RecuperarContrasenaRequest
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        public string Correo { get; set; } = string.Empty;
    }
}
