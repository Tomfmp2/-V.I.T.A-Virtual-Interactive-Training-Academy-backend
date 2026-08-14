using Vita.Api.Dtos.Roles;

namespace Vita.Api.Services;

public interface IRoleService
{
    Task<IReadOnlyList<RoleDto>> GetAllAsync();
}
