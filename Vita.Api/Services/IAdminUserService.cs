using Vita.Api.Dtos.Users;

namespace Vita.Api.Services;

public enum AdminUserOutcome
{
    Success,
    NotFound,
    EmailExists,
    InvalidRole,
    ValidationError
}

public class AdminUserResult
{
    public AdminUserOutcome Outcome { get; set; }
    public UserResponse? User { get; set; }
    public List<string> Errors { get; set; } = new();
}

public interface IAdminUserService
{
    Task<List<UserResponse>> GetAllAsync();
    Task<UserResponse?> GetByIdAsync(string id);
    Task<AdminUserResult> CreateAsync(CreateUserRequest request);
    Task<AdminUserResult> UpdateAsync(string id, UpdateUserRequest request);
    Task<AdminUserResult> UpdateStatusAsync(string id, UpdateUserStatusRequest request);
    Task<AdminUserResult> UpdateRoleAsync(string id, UpdateUserRoleRequest request);
}
