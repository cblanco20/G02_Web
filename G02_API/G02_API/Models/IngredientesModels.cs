using System.ComponentModel.DataAnnotations;

namespace G02_API.Models
{
    public class UnidadConsulta
    {
        public int IdUnidad { get; set; }

        public string NombreUnidad { get; set; } = string.Empty;

        public string Abreviatura { get; set; } = string.Empty;
    }

    public class IngredienteConsulta
    {
        public int IdIngrediente { get; set; }

        public string NombreIngrediente { get; set; } = string.Empty;

        public int IdUnidad { get; set; }

        public string NombreUnidad { get; set; } = string.Empty;

        public string Abreviatura { get; set; } = string.Empty;

        public bool Activo { get; set; }
    }

    public class IngredienteRequest
    {
        [Required(ErrorMessage = "El nombre del ingrediente es obligatorio.")]
        [StringLength(150)]
        public string NombreIngrediente { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una unidad de medida.")]
        public int IdUnidad { get; set; }
    }

    public class CambiarEstadoIngrediente
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un ingrediente válido.")]
        public int IdIngrediente { get; set; }

        [Required]
        public bool? Activo { get; set; }
    }
}
