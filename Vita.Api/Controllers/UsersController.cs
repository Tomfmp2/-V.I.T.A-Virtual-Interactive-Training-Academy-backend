using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vita.Api.Dtos.Users;
using Vita.Api.Services;

namespace Vita.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : BaseApiController
{
    private readonly IAdminUserService _service;

    public UsersController(IAdminUserService service)
    {
        _service = service;
    }

    // GET /api/users — solo Admin
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    // GET /api/users/{id} — solo Admin
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _service.GetByIdAsync(id);
        return user is null
            ? ApiError(404, "Usuario no encontrado.")
            : Ok(user);
    }

    // POST /api/users — solo Admin
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var result = await _service.CreateAsync(request);
        return result.Outcome switch
        {
            AdminUserOutcome.EmailExists => ApiError(409, "El email ya está registrado."),
            AdminUserOutcome.InvalidRole => ApiError(400, "Rol inválido. Usa Admin, Instructor o Estudiante."),
            AdminUserOutcome.ValidationError => ApiError(400, string.Join(" ", result.Errors)),
            _ => StatusCode(StatusCodes.Status201Created, result.User)
        };
    }

    // PUT /api/users/{id} — solo Admin
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Outcome switch
        {
            AdminUserOutcome.NotFound => ApiError(404, "Usuario no encontrado."),
            AdminUserOutcome.EmailExists => ApiError(409, "El email ya está registrado."),
            AdminUserOutcome.ValidationError => ApiError(400, string.Join(" ", result.Errors)),
            _ => Ok(result.User)
        };
    }

    // PATCH /api/users/{id}/status — solo Admin (borrado lógico vía Activo)
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateUserStatusRequest request)
    {
        var result = await _service.UpdateStatusAsync(id, request);
        return result.Outcome switch
        {
            AdminUserOutcome.NotFound => ApiError(404, "Usuario no encontrado."),
            AdminUserOutcome.ValidationError => ApiError(400, string.Join(" ", result.Errors)),
            _ => Ok(result.User)
        };
    }

    // PATCH /api/users/{id}/role — solo Admin
    [HttpPatch("{id}/role")]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateUserRoleRequest request)
    {
        var result = await _service.UpdateRoleAsync(id, request);
        return result.Outcome switch
        {
            AdminUserOutcome.NotFound => ApiError(404, "Usuario no encontrado."),
            AdminUserOutcome.InvalidRole => ApiError(400, "Rol inválido. Usa Admin, Instructor o Estudiante."),
            AdminUserOutcome.ValidationError => ApiError(400, string.Join(" ", result.Errors)),
            _ => Ok(result.User)
        };
    }
}
