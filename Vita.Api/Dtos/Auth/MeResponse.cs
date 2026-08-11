namespace Vita.Api.Dtos.Auth;


// Respuesta del GET /api/auth/me - devuelve los campos que pide la tarjeta (id, nombre, emai, rol, activo)
public class MeResponse
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; } 
}