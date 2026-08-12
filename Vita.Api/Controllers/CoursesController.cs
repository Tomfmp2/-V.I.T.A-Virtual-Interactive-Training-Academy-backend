using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vita.Api.Dtos.Courses;
using Vita.Api.Services;

namespace Vita.Api.Controllers;

[ApiController]
[Route("api/courses")]
[Authorize]
public class CoursesController : BaseApiController
{
    private readonly ICourseService _service;

    public CoursesController(ICourseService service)
    {
        _service = service;
    }

    private string? CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    private string CurrentRole =>
        User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role") ?? "Estudiante";

    // GET /api/courses — cualquier autenticado (filtrado por rol en el servicio)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = CurrentUserId;
        if (userId is null)
            return ApiError(401, "No autorizado.");

        return Ok(await _service.GetAllAsync(userId, CurrentRole));
    }

    // GET /api/courses/me — solo Instructor (declarado ANTES de /{id:int})
    [HttpGet("me")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> GetMine()
    {
        var userId = CurrentUserId;
        if (userId is null)
            return ApiError(401, "No autorizado.");

        return Ok(await _service.GetMineAsync(userId));
    }

    // GET /api/courses/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return ApiError(401, "No autorizado.");

        var course = await _service.GetByIdAsync(id, userId, CurrentRole);
        return course is null
            ? ApiError(404, "Curso no encontrado.")
            : Ok(course);
    }

    // POST /api/courses — solo Instructor; IdInstructor sale del token
    [HttpPost]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Create([FromBody] CourseCreateRequest request)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return ApiError(401, "No autorizado.");

        var result = await _service.CreateAsync(request, userId);
        return result.Outcome switch
        {
            CourseOutcome.CategoryNotFound => ApiError(400, "La categoría no existe o está inactiva."),
            CourseOutcome.NivelNotFound => ApiError(400, "El nivel no existe."),
            CourseOutcome.TituloExists => ApiError(409, "Ya tienes un curso con ese título."),
            _ => CreatedAtAction(nameof(GetById), new { id = result.Course!.Id }, result.Course)
        };
    }

    // PUT /api/courses/{id} — solo el dueño (Instructor); Admin no edita ajenos aquí
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Update(int id, [FromBody] CourseUpdateRequest request)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return ApiError(401, "No autorizado.");

        var result = await _service.UpdateAsync(id, request, userId);
        return result.Outcome switch
        {
            CourseOutcome.NotFound => ApiError(404, "Curso no encontrado."),
            CourseOutcome.Forbidden => ApiError(403, "No tienes permiso para modificar este curso."),
            CourseOutcome.CategoryNotFound => ApiError(400, "La categoría no existe o está inactiva."),
            CourseOutcome.NivelNotFound => ApiError(400, "El nivel no existe."),
            CourseOutcome.TituloExists => ApiError(409, "Ya tienes un curso con ese título."),
            _ => Ok(result.Course)
        };
    }

    // PATCH /api/courses/{id}/status — solo el dueño
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] CourseStatusRequest request)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return ApiError(401, "No autorizado.");

        var result = await _service.ChangeStatusAsync(id, request, userId);
        return result.Outcome switch
        {
            CourseOutcome.InvalidEstado => ApiError(400, "Estado inválido. Valores permitidos: borrador, publicado."),
            CourseOutcome.NotFound => ApiError(404, "Curso no encontrado."),
            CourseOutcome.Forbidden => ApiError(403, "No tienes permiso para modificar este curso."),
            _ => Ok(result.Course)
        };
    }

    // DELETE /api/courses/{id} — Instructor dueño o Admin
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return ApiError(401, "No autorizado.");

        var result = await _service.DeleteAsync(id, userId, CurrentRole);
        return result.Outcome switch
        {
            CourseOutcome.NotFound => ApiError(404, "Curso no encontrado."),
            CourseOutcome.Forbidden => ApiError(403, "No tienes permiso para modificar este curso."),
            CourseOutcome.HasEnrollments => ApiError(409, "No se puede eliminar: el curso tiene estudiantes inscritos."),
            CourseOutcome.HasLessons => ApiError(409, "No se puede eliminar: el curso tiene lecciones asociadas."),
            _ => NoContent()
        };
    }
}
