using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Categories;

public class CategoryRequest
{
    [Required]
    [StringLength(60, MinimumLength = 3)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Descripcion { get; set; }

    [MaxLength(255)]
    public string? IconoUrl { get; set; }
}
