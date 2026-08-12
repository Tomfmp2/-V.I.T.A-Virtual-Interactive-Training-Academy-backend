using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Users;

public class UpdateUserRoleRequest
{
    [Required]
    [StringLength(50)]
    public string Rol { get; set; } = string.Empty;
}
