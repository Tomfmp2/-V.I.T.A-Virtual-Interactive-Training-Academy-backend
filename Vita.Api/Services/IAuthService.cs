using Vita.Api.Dtos.Auth;

namespace Vita.Api.Services;

public enum RegisterStatus
{
    Success,
    EmailExist,
    ValidationError
}

public class RegisterResult
{
    public RegisterStatus Status { get; set; }
    public RegisterResponse? Response { get; set; }
    public List<string> Errors { get; set; }= new();
}

public enum LoginStatus
{
    Success,
    InvalidCredentials,
    Inactive
}

public class LoginResult
{
    public LoginStatus Status { get; set; }
    public LoginResponse? Response { get; set; }
}


public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request);
    Task<LoginResult> LoginAsync(LoginRequest request);
}
