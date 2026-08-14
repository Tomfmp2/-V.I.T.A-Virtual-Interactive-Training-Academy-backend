using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Users;

public class CreateUserRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El apellido debe tener entre 3 y 100 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [StringLength(50, ErrorMessage = "El rol no puede superar 50 caracteres.")]
    public string Rol { get; set; } = string.Empty;
}
