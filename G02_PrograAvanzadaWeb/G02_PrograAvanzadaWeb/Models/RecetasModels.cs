using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace G02_PrograAvanzadaWeb.Models
{
    public class ApiMensaje
    {
        public string Mensaje { get; set; } = string.Empty;
    }

    public class CategoriaItem
    {
        public int IdCategoria { get; set; }

        public string NombreCategoria { get; set; } = string.Empty;
    }

    public class UnidadItem
    {
        public int IdUnidad { get; set; }

        public string NombreUnidad { get; set; } = string.Empty;

        public string Abreviatura { get; set; } = string.Empty;
    }

    public class IngredienteItem
    {
        public int IdIngrediente { get; set; }

        public string NombreIngrediente { get; set; } = string.Empty;

        public int IdUnidad { get; set; }

        public string NombreUnidad { get; set; } = string.Empty;

        public string Abreviatura { get; set; } = string.Empty;

        public bool Activo { get; set; }
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

    public class RecetaIngredienteItem
    {
        public int IdIngrediente { get; set; }

        public string NombreIngrediente { get; set; } = string.Empty;

        public decimal Cantidad { get; set; }

        public string NombreUnidad { get; set; } = string.Empty;

        public string Abreviatura { get; set; } = string.Empty;
    }

    public class RecetaDetalleItem
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

        public List<RecetaIngredienteItem> Ingredientes { get; set; } = new();
    }

    public class RecetasIndexViewModel
    {
        public List<RecetaResumen> Recetas { get; set; } = new();

        public List<CategoriaItem> Categorias { get; set; } = new();

        public int? IdCategoria { get; set; }

        public string? Busqueda { get; set; }
    }

    public class RecetaIngredienteFormulario
    {
        public int IdIngrediente { get; set; }

        public string? Cantidad { get; set; }
    }

    public class FilaIngredienteViewModel
    {
        public int Indice { get; set; }

        public RecetaIngredienteFormulario Fila { get; set; } = new();

        public List<IngredienteItem> Opciones { get; set; } = new();
    }

    public class RecetaFormulario
    {
        public int IdReceta { get; set; }

        [Required(ErrorMessage = "El nombre de la receta es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        public string NombreReceta { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string? Descripcion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una categoría.")]
        public int IdCategoria { get; set; }

        [Range(1, 1440, ErrorMessage = "El tiempo debe estar entre 1 y 1440 minutos.")]
        public int TiempoPreparacion { get; set; }

        [Range(1, 100, ErrorMessage = "Las porciones deben estar entre 1 y 100.")]
        public int Porciones { get; set; }

        [Required(ErrorMessage = "Las instrucciones son obligatorias.")]
        public string Instrucciones { get; set; } = string.Empty;

        public List<RecetaIngredienteFormulario> Ingredientes { get; set; } = new();

        [BindNever]
        [ValidateNever]
        public List<CategoriaItem> Categorias { get; set; } = new();

        [BindNever]
        [ValidateNever]
        public List<IngredienteItem> IngredientesDisponibles { get; set; } = new();
    }

    public class IngredientesIndexViewModel
    {
        public List<IngredienteItem> Ingredientes { get; set; } = new();

        public List<UnidadItem> Unidades { get; set; } = new();
    }

    public class IngredienteFormulario
    {
        public int IdIngrediente { get; set; }

        [Required(ErrorMessage = "El nombre del ingrediente es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        public string NombreIngrediente { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una unidad de medida.")]
        public int IdUnidad { get; set; }

        [BindNever]
        [ValidateNever]
        public List<UnidadItem> Unidades { get; set; } = new();
    }
}
