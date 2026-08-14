// Implementacion
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Vita.Api.Common;
using Vita.Api.Dtos.Auth;
using Vita.Api.Entities;

namespace Vita.Api.Services;
 
public class AuthService : IAuthService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public AuthService(
        UserManager<Usuario> userManager,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _configuration = configuration;
        _environment = environment;
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
            Apellido = request.Apellido,
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
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Rol = "Estudiante",
                Activo = usuario.Activo
            }
        };


    }

    public async Task<LoginResult> LoginAsync(LoginRequest request)
    {
        // 1) Buscar el usuario por email
        var usuario = await _userManager.FindByEmailAsync(request.Email);
        if (usuario is null)
            return new LoginResult { Status = LoginStatus.InvalidCredentials };

        // 2) Verificar el password
        var passwordOk = await _userManager.CheckPasswordAsync(usuario, request.Password);
        if (!passwordOk)
            return new LoginResult { Status = LoginStatus.InvalidCredentials };

        // 3) Usuario inactivo -> 403
        if (!usuario.Activo)
            return new LoginResult { Status = LoginStatus.Inactive };

        // 4) Obtener su rol
        var roles = await _userManager.GetRolesAsync(usuario);
        var rol = roles.FirstOrDefault() ?? "Estudiante";

        // 5) Generar el token
        var (token, expiraEn) = GenerarToken(usuario, rol);

        return new LoginResult
        {
            Status = LoginStatus.Success,
            Response = new LoginResponse
            {
                Token = token,
                ExpiraEn = expiraEn,
                Usuario = new UsuarioLoginDto
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Email = usuario.Email!,
                    Rol = rol
                }
            }
        };
    }

    private (string token, int expiraEn) GenerarToken(Usuario usuario, string rol)
    {
        var jwt = _configuration.GetSection("Jwt");
        var expiraEn = int.Parse(jwt["ExpireSeconds"]!);

        // Claims que van DENTRO del token
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email!),
            new Claim("role", rol)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(expiraEn),
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEn);
    }

    public async Task<MeResponse?> GetMeAsync(string userId)
    {
        // Se busca el usuario por el Id(el que trae el claim sub del token)
        var usuario = await _userManager.FindByIdAsync(userId);
        // Si usuario no existe return null
        if (usuario is null)
            return null;
            // Saca roles para anadirlo en la respuesta
        var roles = await _userManager.GetRolesAsync(usuario);
        var rol = roles.FirstOrDefault() ?? "Estudiante";

        return MapMe(usuario, rol);
    }

    public async Task<ProfileResult> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var usuario = await _userManager.FindByIdAsync(userId);
        if (usuario is null)
            return new ProfileResult { Outcome = ProfileOutcome.NotFound };

        if (!usuario.Activo)
            return new ProfileResult { Outcome = ProfileOutcome.Inactive };

        usuario.Nombre = request.Nombre.Trim();
        usuario.Apellido = request.Apellido.Trim();
        usuario.Telefono = NormalizeTelefono(request.Telefono);
        usuario.CodigoPais = string.IsNullOrWhiteSpace(request.CodigoPais)
            ? null
            : request.CodigoPais.Trim();

        var update = await _userManager.UpdateAsync(usuario);
        if (!update.Succeeded)
        {
            return new ProfileResult
            {
                Outcome = ProfileOutcome.ValidationError,
                Errors = update.Errors.Select(e => e.Description).ToList()
            };
        }

        var roles = await _userManager.GetRolesAsync(usuario);
        var rol = roles.FirstOrDefault() ?? "Estudiante";

        return new ProfileResult
        {
            Outcome = ProfileOutcome.Success,
            Profile = MapMe(usuario, rol)
        };
    }

    public async Task<ProfileResult> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        var usuario = await _userManager.FindByIdAsync(userId);
        if (usuario is null)
            return new ProfileResult { Outcome = ProfileOutcome.NotFound };

        if (!usuario.Activo)
            return new ProfileResult { Outcome = ProfileOutcome.Inactive };

        if (request.NuevaContrasena != request.ConfirmarContrasena)
            return new ProfileResult { Outcome = ProfileOutcome.PasswordMismatch };

        var passwordOk = await _userManager.CheckPasswordAsync(usuario, request.ContrasenaActual);
        if (!passwordOk)
            return new ProfileResult { Outcome = ProfileOutcome.WrongPassword };

        var result = await _userManager.ChangePasswordAsync(
            usuario,
            request.ContrasenaActual,
            request.NuevaContrasena);

        if (!result.Succeeded)
        {
            return new ProfileResult
            {
                Outcome = ProfileOutcome.ValidationError,
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        return new ProfileResult
        {
            Outcome = ProfileOutcome.Success,
            Message = "Contraseña actualizada correctamente."
        };
    }

    public async Task<ProfileResult> UploadPhotoAsync(string userId, IFormFile file)
    {
        var usuario = await _userManager.FindByIdAsync(userId);
        if (usuario is null)
            return new ProfileResult { Outcome = ProfileOutcome.NotFound };

        if (!usuario.Activo)
            return new ProfileResult { Outcome = ProfileOutcome.Inactive };

        if (!ImageUploadHelper.TryValidate(file, out var extension, out var error))
        {
            return new ProfileResult
            {
                Outcome = ProfileOutcome.FileInvalid,
                Errors = [error!]
            };
        }

        if (!await ImageUploadHelper.HasImageSignatureAsync(file, extension))
        {
            return new ProfileResult
            {
                Outcome = ProfileOutcome.FileInvalid,
                Errors = ["Formato no permitido. Usa JPG, PNG o WEBP."]
            };
        }

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var profilesDir = Path.Combine(webRoot, "uploads", "profiles");
        Directory.CreateDirectory(profilesDir);

        ImageUploadHelper.DeleteExistingFiles(profilesDir, userId);

        var fileName = $"{userId}{extension}";
        var physicalPath = Path.Combine(profilesDir, fileName);
        await using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var fotoUrl = $"/uploads/profiles/{fileName}";
        usuario.FotoUrl = fotoUrl;

        var update = await _userManager.UpdateAsync(usuario);
        if (!update.Succeeded)
        {
            if (File.Exists(physicalPath))
                File.Delete(physicalPath);

            return new ProfileResult
            {
                Outcome = ProfileOutcome.ValidationError,
                Errors = update.Errors.Select(e => e.Description).ToList()
            };
        }

        return new ProfileResult
        {
            Outcome = ProfileOutcome.Success,
            Photo = new UploadPhotoResponse { FotoUrl = fotoUrl }
        };
    }

    private static string? NormalizeTelefono(string? telefono)
    {
        if (string.IsNullOrWhiteSpace(telefono))
            return null;

        var digits = new string(telefono.Where(char.IsDigit).ToArray());
        return digits.Length == 0 ? null : digits;
    }

    private static MeResponse MapMe(Usuario usuario, string rol) =>
        new()
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email!,
            Rol = rol,
            Activo = usuario.Activo,
            FotoUrl = usuario.FotoUrl,
            Telefono = usuario.Telefono,
            CodigoPais = usuario.CodigoPais
        };




}