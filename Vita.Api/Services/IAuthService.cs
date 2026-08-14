using Microsoft.AspNetCore.Http;
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

public enum ProfileOutcome
{
    Success,
    NotFound,
    Inactive,
    ValidationError,
    WrongPassword,
    PasswordMismatch,
    FileInvalid
}

public class ProfileResult
{
    public ProfileOutcome Outcome { get; set; }
    public MeResponse? Profile { get; set; }
    public UploadPhotoResponse? Photo { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
}

public interface IAuthService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request);
    Task<LoginResult> LoginAsync(LoginRequest request);
    Task<MeResponse?> GetMeAsync(string userId);
    Task<ProfileResult> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    Task<ProfileResult> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    Task<ProfileResult> UploadPhotoAsync(string userId, IFormFile file);
}
