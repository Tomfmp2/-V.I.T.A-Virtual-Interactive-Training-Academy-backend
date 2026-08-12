using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Lessons;

public class LessonRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    [MaxLength(255)]
    [Url]
    public string? Recurso { get; set; }

    [Range(1, int.MaxValue)]
    public int Orden { get; set; } = 1;
}
