using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vita.Api.Dtos.Users;
using Vita.Api.Entities;

namespace Vita.Api.Services;

public class AdminUserService : IAdminUserService
{
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin",
        "Instructor",
        "Estudiante"
    };

    private readonly UserManager<Usuario> _userManager;

    public AdminUserService(UserManager<Usuario> userManager)
    {
        _userManager = userManager;
    }

    public async Task<List<UserResponse>> GetAllAsync()
    {
        var usuarios = await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        var result = new List<UserResponse>(usuarios.Count);
        foreach (var usuario in usuarios)
            result.Add(await MapAsync(usuario));

        return result;
    }

    public async Task<UserResponse?> GetByIdAsync(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        return usuario is null ? null : await MapAsync(usuario);
    }

    public async Task<AdminUserResult> CreateAsync(CreateUserRequest request)
    {
        var rol = NormalizeRole(request.Rol);
        if (rol is null)
            return new AdminUserResult { Outcome = AdminUserOutcome.InvalidRole };

        var email = request.Email.Trim();
        var existente = await _userManager.FindByEmailAsync(email);
        if (existente is not null)
            return new AdminUserResult { Outcome = AdminUserOutcome.EmailExists };

        var usuario = new Usuario
        {
            UserName = email,
            Email = email,
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            Activo = true,
            CreatedAt = DateTime.UtcNow
        };

        var create = await _userManager.CreateAsync(usuario, request.Password);
        if (!create.Succeeded)
        {
            return new AdminUserResult
            {
                Outcome = AdminUserOutcome.ValidationError,
                Errors = create.Errors.Select(e => e.Description).ToList()
            };
        }

        var addRole = await _userManager.AddToRoleAsync(usuario, rol);
        if (!addRole.Succeeded)
        {
            return new AdminUserResult
            {
                Outcome = AdminUserOutcome.ValidationError,
                Errors = addRole.Errors.Select(e => e.Description).ToList()
            };
        }

        return new AdminUserResult
        {
            Outcome = AdminUserOutcome.Success,
            User = await MapAsync(usuario)
        };
    }

    public async Task<AdminUserResult> UpdateAsync(string id, UpdateUserRequest request)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario is null)
            return new AdminUserResult { Outcome = AdminUserOutcome.NotFound };

        var email = request.Email.Trim();
        var otro = await _userManager.FindByEmailAsync(email);
        if (otro is not null && otro.Id != id)
            return new AdminUserResult { Outcome = AdminUserOutcome.EmailExists };

        usuario.Nombre = request.Nombre.Trim();
        usuario.Apellido = request.Apellido.Trim();
        usuario.Email = email;
        usuario.UserName = email;

        var update = await _userManager.UpdateAsync(usuario);
        if (!update.Succeeded)
        {
            return new AdminUserResult
            {
                Outcome = AdminUserOutcome.ValidationError,
                Errors = update.Errors.Select(e => e.Description).ToList()
            };
        }

        return new AdminUserResult
        {
            Outcome = AdminUserOutcome.Success,
            User = await MapAsync(usuario)
        };
    }

    public async Task<AdminUserResult> UpdateStatusAsync(string id, UpdateUserStatusRequest request)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario is null)
            return new AdminUserResult { Outcome = AdminUserOutcome.NotFound };

        usuario.Activo = request.Activo!.Value;

        var update = await _userManager.UpdateAsync(usuario);
        if (!update.Succeeded)
        {
            return new AdminUserResult
            {
                Outcome = AdminUserOutcome.ValidationError,
                Errors = update.Errors.Select(e => e.Description).ToList()
            };
        }

        return new AdminUserResult
        {
            Outcome = AdminUserOutcome.Success,
            User = await MapAsync(usuario)
        };
    }

    public async Task<AdminUserResult> UpdateRoleAsync(string id, UpdateUserRoleRequest request)
    {
        var rol = NormalizeRole(request.Rol);
        if (rol is null)
            return new AdminUserResult { Outcome = AdminUserOutcome.InvalidRole };

        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario is null)
            return new AdminUserResult { Outcome = AdminUserOutcome.NotFound };

        var rolesActuales = await _userManager.GetRolesAsync(usuario);
        if (rolesActuales.Count > 0)
        {
            var remove = await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            if (!remove.Succeeded)
            {
                return new AdminUserResult
                {
                    Outcome = AdminUserOutcome.ValidationError,
                    Errors = remove.Errors.Select(e => e.Description).ToList()
                };
            }
        }

        var add = await _userManager.AddToRoleAsync(usuario, rol);
        if (!add.Succeeded)
        {
            return new AdminUserResult
            {
                Outcome = AdminUserOutcome.ValidationError,
                Errors = add.Errors.Select(e => e.Description).ToList()
            };
        }

        return new AdminUserResult
        {
            Outcome = AdminUserOutcome.Success,
            User = await MapAsync(usuario)
        };
    }

    private async Task<UserResponse> MapAsync(Usuario usuario)
    {
        var roles = await _userManager.GetRolesAsync(usuario);
        var rol = roles.FirstOrDefault() ?? "Estudiante";

        return new UserResponse
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email ?? string.Empty,
            Rol = rol,
            Activo = usuario.Activo
        };
    }

    private static string? NormalizeRole(string? rol)
    {
        if (string.IsNullOrWhiteSpace(rol))
            return null;

        var trimmed = rol.Trim();
        if (!AllowedRoles.Contains(trimmed))
            return null;

        // Canonical names as seeded in Identity
        return AllowedRoles.First(r => r.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
    }
}
