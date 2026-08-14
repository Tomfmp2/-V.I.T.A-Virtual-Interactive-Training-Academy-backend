using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Courses;

public class CourseUpdateRequest
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "El título debe tener entre 5 y 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La categoría no es válida.")]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "El nivel es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El nivel no es válido.")]
    public int IdNivel { get; set; }

    [MaxLength(300, ErrorMessage = "La descripción corta no puede superar 300 caracteres.")]
    public string? DescripcionCorta { get; set; }

    public string? DescripcionLarga { get; set; }

    [MaxLength(255, ErrorMessage = "La URL de portada no puede superar 255 caracteres.")]
    public string? ImagenPortadaUrl { get; set; }

    [Range(1, 100000, ErrorMessage = "La duración estimada debe estar entre 1 y 100000 minutos.")]
    public int? DuracionEstimadaMin { get; set; }
}
