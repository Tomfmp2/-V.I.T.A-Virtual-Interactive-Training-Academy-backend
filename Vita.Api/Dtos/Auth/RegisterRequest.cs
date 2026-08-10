using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Auth;

public class RegisterRequest {
    [Required]
    [StringLength(100, MinimumLength =3)]
    public string Nombre { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

}

