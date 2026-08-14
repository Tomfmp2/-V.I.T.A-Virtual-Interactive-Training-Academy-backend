using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vita.Api.Dtos.Lessons;
using Vita.Api.Services;

namespace Vita.Api.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/lessons")]
[Authorize] // todo requiere autenticación
public class LessonsController : BaseApiController
{
    private readonly ILessonService _service;

    public LessonsController(ILessonService service)
    {
        _service = service;
    }

    // Id del usuario del token (claim sub, mapeado a NameIdentifier)
    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? "";

    private string CurrentRole =>
        User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role) ?? "";

    private const string NotOwnerMsg = "Solo el instructor dueño del curso puede gestionar sus lecciones.";

    // GET /api/courses/{courseId}/lessons  (autenticado) — ordenado por orden
    [HttpGet]
    public async Task<IActionResult> GetAll(int courseId)
    {
        var result = await _service.GetByCourseAsync(courseId);
        return result.Outcome switch
        {
            LessonOutcome.CourseNotFound => ApiError(404, "Curso no encontrado."),
            _ => Ok(result.Lessons)
        };
    }

    // GET /api/courses/{courseId}/lessons/{id}  (autenticado)
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int courseId, int id)
    {
        var result = await _service.GetByIdAsync(courseId, id);
        return result.Outcome switch
        {
            LessonOutcome.CourseNotFound => ApiError(404, "Curso no encontrado."),
            LessonOutcome.LessonNotFound => ApiError(404, "Lección no encontrada."),
            _ => Ok(result.Lesson)
        };
    }

    // POST /api/courses/{courseId}/lessons  (solo dueño del curso)
    [HttpPost]
    public async Task<IActionResult> Create(int courseId, [FromBody] LessonRequest request)
    {
        var result = await _service.CreateAsync(courseId, UserId, CurrentRole, request);
        return result.Outcome switch
        {
            LessonOutcome.CourseNotFound => ApiError(404, "Curso no encontrado."),
            LessonOutcome.NotOwner => ApiError(403, NotOwnerMsg),
            _ => StatusCode(StatusCodes.Status201Created, result.Lesson)
        };
    }

    // PUT /api/courses/{courseId}/lessons/{id}  (solo dueño del curso)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int courseId, int id, [FromBody] LessonRequest request)
    {
        var result = await _service.UpdateAsync(courseId, id, UserId, CurrentRole, request);
        return result.Outcome switch
        {
            LessonOutcome.CourseNotFound => ApiError(404, "Curso no encontrado."),
            LessonOutcome.NotOwner => ApiError(403, NotOwnerMsg),
            LessonOutcome.LessonNotFound => ApiError(404, "Lección no encontrada."),
            _ => Ok(result.Lesson)
        };
    }

    // DELETE /api/courses/{courseId}/lessons/{id}  (solo dueño del curso)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int courseId, int id)
    {
        var result = await _service.DeleteAsync(courseId, id, UserId, CurrentRole);
        return result.Outcome switch
        {
            LessonOutcome.CourseNotFound => ApiError(404, "Curso no encontrado."),
            LessonOutcome.NotOwner => ApiError(403, NotOwnerMsg),
            LessonOutcome.LessonNotFound => ApiError(404, "Lección no encontrada."),
            _ => NoContent()
        };
    }
}
