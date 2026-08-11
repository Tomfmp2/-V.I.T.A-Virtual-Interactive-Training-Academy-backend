using Microsoft.AspNetCore.Mvc;
using Vita.Api.Dtos.Auth;
using Vita.Api.Services;

namespace Vita.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
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
          RegisterStatus.EmailExist => Conflict(new {error = "El Email ya esta registrado."}),
          RegisterStatus.ValidationError => BadRequest(new { errors = result.Errors}),
          _                              => StatusCode(StatusCodes.Status201Created, result.Response)
        };
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        return result.Status switch
        {
            LoginStatus.InvalidCredentials => Unauthorized(new { error = "Credenciales inválidas." }),
            LoginStatus.Inactive           => StatusCode(StatusCodes.Status403Forbidden, new { error = "Usuario inactivo." }),
            _                              => Ok(result.Response)
        };
    }
}