using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Levels;

public class LevelRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
}
