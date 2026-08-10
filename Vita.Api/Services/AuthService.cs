// Implementacion
using Microsoft.AspNetCore.Identity;
using Vita.Api.Dtos.Auth;
using Vita.Api.Entities;

namespace Vita.Api.Services;
 
public class AuthService : IAuthService
{
    private readonly UserManager<Usuario> _userManager;
    
    public AuthService(UserManager<Usuario> userManager)
    {
        _userManager = userManager;
    }
    public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
    {
        // Email ya registrado? -> 409
        var existente = await _userManager.FindByEmailAsync(request.Email);
        // Si ya existe devuelve EmailExist
        if (existente is not null)
        {
            return new RegisterResult { Status = RegisterStatus.EmailExist } ;
        }

        // Crear Usuario (Identity hashea el password solo)
        var usuario = new Usuario
        {
            UserName = request.Email,
            Email = request.Email,
            Nombre = request.Nombre,
            Activo = true
        };

        var resultado = await _userManager.CreateAsync(usuario, request.Password);
        if (!resultado.Succeeded)
        {
            return new RegisterResult
            {
                Status = RegisterStatus.ValidationError,
                Errors = resultado.Errors.Select (e => e.Description).ToList()
            };
        }
        // Frozar rol Estudiante ( Ignora cualquier rol indicando por el cliente)
        await _userManager.AddToRoleAsync(usuario, "Estudiante");
        // Respuesta sin el password ni el hash
        return new RegisterResult
        {
            Status = RegisterStatus.Success,
            Response = new RegisterResponse
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = "Estudiante",
                Activo = usuario.Activo
            }
        };


    }
}