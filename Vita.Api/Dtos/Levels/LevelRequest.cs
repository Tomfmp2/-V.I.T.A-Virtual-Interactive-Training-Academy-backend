using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Levels;

public class LevelRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Nombre { get; set; } = string.Empty;
}
