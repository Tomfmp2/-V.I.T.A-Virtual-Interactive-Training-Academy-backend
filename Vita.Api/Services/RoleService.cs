using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vita.Api.Dtos.Roles;

namespace Vita.Api.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleService(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<IReadOnlyList<RoleDto>> GetAllAsync()
    {
        return await _roleManager.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name!
            })
            .ToListAsync();
    }
}
