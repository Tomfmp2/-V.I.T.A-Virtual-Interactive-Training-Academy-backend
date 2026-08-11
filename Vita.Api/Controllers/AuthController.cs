using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
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

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me ()
    {
        // userId llega en el claim "sub" del token
        //Cuando se valida el JWT, "sub" mapea ClaimsType.NameIdentifier
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) // el claim sub ser renombra a ClaimsTypes.NameIdentifier por eso se lee primero este
                        ?? User.FindFirstValue("sub"); // y este se lee como respaldo "sub"
        
        if (userId is null)
            return Unauthorized();

        var me = await _authService.GetMeAsync(userId);
        if ( me is null)
            return Unauthorized();
        return Ok(me);
    }

    [Authorize] // exije un Bearer token valido. Si no lo trae ASP.NET responde 401
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        //Logout stateless: o sea el servidor No invalida el token, solo responde 200 ok
        // el cliente (front) debe borrar el token de su almacenamiento
        return Ok( new{ message = "Sesion cerrada" });
    }

}



