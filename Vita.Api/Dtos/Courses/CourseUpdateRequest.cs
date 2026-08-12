using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Courses;

public class CourseUpdateRequest
{
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue)]
    public int IdCategoria { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int IdNivel { get; set; }

    [MaxLength(300)]
    public string? DescripcionCorta { get; set; }

    public string? DescripcionLarga { get; set; }

    [MaxLength(255)]
    public string? ImagenPortadaUrl { get; set; }

    [Range(1, 100000)]
    public int? DuracionEstimadaMin { get; set; }
}
