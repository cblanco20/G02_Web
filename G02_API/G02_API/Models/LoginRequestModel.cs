using System.ComponentModel.DataAnnotations;

namespace G02_API.Models
{
    public class LoginRequestModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Contrasena { get; set; } = string.Empty;
    }
}