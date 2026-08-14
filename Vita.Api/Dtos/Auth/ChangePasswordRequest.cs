using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Vita.Api.Dtos.Auth;

public class ChangePasswordRequest
{
    [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
    [JsonPropertyName("contraseñaActual")]
    public string ContrasenaActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [JsonPropertyName("nuevaContraseña")]
    public string NuevaContrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes confirmar la nueva contraseña.")]
    [JsonPropertyName("confirmarContraseña")]
    public string ConfirmarContrasena { get; set; } = string.Empty;
}
