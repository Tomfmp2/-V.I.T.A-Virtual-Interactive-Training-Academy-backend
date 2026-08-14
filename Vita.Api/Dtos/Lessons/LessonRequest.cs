using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Lessons;

public class LessonRequest
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    [MaxLength(255, ErrorMessage = "El recurso no puede superar 255 caracteres.")]
    [Url(ErrorMessage = "El recurso debe ser una URL válida (http o https).")]
    public string? Recurso { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El orden debe ser un número mayor o igual a 1.")]
    public int Orden { get; set; } = 1;
}
