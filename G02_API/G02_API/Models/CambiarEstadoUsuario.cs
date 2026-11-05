using System.ComponentModel.DataAnnotations;

namespace G02_API.Models
{
    public class CambiarEstadoUsuario
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un usuario válido.")]
        public int IdUsuario { get; set; }

        [Required]
        public bool? Activo { get; set; }
    }
}
