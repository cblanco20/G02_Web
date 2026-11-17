using System.ComponentModel.DataAnnotations;

namespace G02_API.Models
{
    public class CategoriaConsulta
    {
        public int IdCategoria { get; set; }

        public string NombreCategoria { get; set; } = string.Empty;
    }

    public class RecetaResumen
    {
        public int IdReceta { get; set; }

        public string NombreReceta { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public int IdCategoria { get; set; }

        public string NombreCategoria { get; set; } = string.Empty;

        public int TiempoPreparacion { get; set; }

        public int Porciones { get; set; }

        public DateTime FechaCreacion { get; set; }

        public int TotalIngredientes { get; set; }
    }

    public class RecetaIngredienteDetalle
    {
        public int IdIngrediente { get; set; }

        public string NombreIngrediente { get; set; } = string.Empty;

        public decimal Cantidad { get; set; }

        public string NombreUnidad { get; set; } = string.Empty;

        public string Abreviatura { get; set; } = string.Empty;
    }

    public class RecetaDetalle
    {
        public int IdReceta { get; set; }

        public string NombreReceta { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public string Instrucciones { get; set; } = string.Empty;

        public int IdCategoria { get; set; }

        public string NombreCategoria { get; set; } = string.Empty;

        public int TiempoPreparacion { get; set; }

        public int Porciones { get; set; }

        public DateTime FechaCreacion { get; set; }

        public List<RecetaIngredienteDetalle> Ingredientes { get; set; } = new();
    }

    public class RecetaIngredienteRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un ingrediente válido.")]
        public int IdIngrediente { get; set; }

        [Range(0.01, 99999999.99, ErrorMessage = "La cantidad debe ser mayor que cero.")]
        public decimal Cantidad { get; set; }
    }

    public class RecetaRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría.")]
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "El nombre de la receta es obligatorio.")]
        [StringLength(150)]
        public string NombreReceta { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Las instrucciones son obligatorias.")]
        public string Instrucciones { get; set; } = string.Empty;

        [Range(1, 1440, ErrorMessage = "El tiempo de preparación debe estar entre 1 y 1440 minutos.")]
        public int TiempoPreparacion { get; set; }

        [Range(1, 100, ErrorMessage = "Las porciones deben estar entre 1 y 100.")]
        public int Porciones { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "La receta debe tener al menos un ingrediente.")]
        public List<RecetaIngredienteRequest> Ingredientes { get; set; } = new();
    }
}
