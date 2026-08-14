using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Users;

public class UpdateUserStatusRequest
{
    [Required(ErrorMessage = "El estado activo es obligatorio.")]
    public bool? Activo { get; set; }
}
