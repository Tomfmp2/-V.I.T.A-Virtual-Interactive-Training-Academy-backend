using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Vita.Api.Dtos.Auth;
using Vita.Api.Services;

namespace Vita.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        return result.Status switch
        {
          RegisterStatus.EmailExist => ApiError(409, "El email ya está registrado."),
          RegisterStatus.ValidationError => ApiError(400, string.Join(" ", result.Errors)),
          _                              => StatusCode(StatusCodes.Status201Created, result.Response)
        };
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        return result.Status switch
        {
            LoginStatus.InvalidCredentials => ApiError(401, "Credenciales inválidas."),
            LoginStatus.Inactive           => ApiError(403, "Usuario inactivo."),
            _                              => Ok(result.Response)
        };
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me ()
    {
        var userId = GetUserId();
        if (userId is null)
            return ApiError(401, "No autorizado.");

        var me = await _authService.GetMeAsync(userId);
        if ( me is null)
             return ApiError(401, "No autorizado.");
        return Ok(me);
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        if (userId is null)
            return ApiError(401, "No autorizado.");

        var result = await _authService.UpdateProfileAsync(userId, request);

        return result.Outcome switch
        {
            ProfileOutcome.NotFound => ApiError(401, "No autorizado."),
            ProfileOutcome.Inactive => ApiError(403, "Usuario inactivo."),
            ProfileOutcome.ValidationError => ApiError(400, string.Join(" ", result.Errors)),
            _ => Ok(result.Profile)
        };
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserId();
        if (userId is null)
            return ApiError(401, "No autorizado.");

        var result = await _authService.ChangePasswordAsync(userId, request);

        return result.Outcome switch
        {
            ProfileOutcome.NotFound => ApiError(401, "No autorizado."),
            ProfileOutcome.Inactive => ApiError(403, "Usuario inactivo."),
            ProfileOutcome.PasswordMismatch => ApiError(400, "Las contraseñas no coinciden."),
            ProfileOutcome.WrongPassword => ApiError(401, "La contraseña actual es incorrecta."),
            ProfileOutcome.ValidationError => ApiError(400, string.Join(" ", result.Errors)),
            _ => Ok(new { message = result.Message })
        };
    }

    [Authorize]
    [HttpPost("me/photo")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<IActionResult> UploadPhoto(IFormFile file)
    {
        var userId = GetUserId();
        if (userId is null)
            return ApiError(401, "No autorizado.");

        var result = await _authService.UploadPhotoAsync(userId, file);

        return result.Outcome switch
        {
            ProfileOutcome.NotFound => ApiError(401, "No autorizado."),
            ProfileOutcome.Inactive => ApiError(403, "Usuario inactivo."),
            ProfileOutcome.FileInvalid => ApiError(400, string.Join(" ", result.Errors)),
            ProfileOutcome.ValidationError => ApiError(400, string.Join(" ", result.Errors)),
            _ => Ok(result.Photo)
        };
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Sesion cerrada" });
    }

    private string? GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub");
}
