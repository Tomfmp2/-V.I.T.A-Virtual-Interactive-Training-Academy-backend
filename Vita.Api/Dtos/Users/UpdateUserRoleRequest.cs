using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Users;

public class UpdateUserRoleRequest
{
    [Required(ErrorMessage = "El rol es obligatorio.")]
    [StringLength(50, ErrorMessage = "El rol no puede superar 50 caracteres.")]
    public string Rol { get; set; } = string.Empty;
}
