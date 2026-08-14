namespace Vita.Api.Dtos.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int ExpiraEn { get; set; }
    public UsuarioLoginDto Usuario { get; set; } = new();
}

public class UsuarioLoginDto
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}