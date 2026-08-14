using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Categories;

public class CategoryRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(60, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(250, ErrorMessage = "La descripción no puede superar 250 caracteres.")]
    public string? Descripcion { get; set; }

    [MaxLength(255, ErrorMessage = "La URL del icono no puede superar 255 caracteres.")]
    public string? IconoUrl { get; set; }
}
