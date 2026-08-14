using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Auth;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El apellido debe tener entre 3 y 100 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El teléfono no puede superar 20 caracteres.")]
    public string? Telefono { get; set; }

    [StringLength(10, ErrorMessage = "El código de país no puede superar 10 caracteres.")]
    [RegularExpression(@"^\+\d{1,4}$", ErrorMessage = "El código de país debe tener formato +NN.")]
    public string? CodigoPais { get; set; }
}
