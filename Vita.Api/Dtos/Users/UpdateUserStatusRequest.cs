using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Users;

public class UpdateUserStatusRequest
{
    [Required]
    public bool? Activo { get; set; }
}
