using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vita.Api.Dtos.Enrollments;
using Vita.Api.Services;

namespace Vita.Api.Controllers;

[ApiController]
[Route("api/enrollments")]
[Authorize] // requiere token; el estudianteId sale del token, no del body
public class EnrollmentsController : BaseApiController
{
    private readonly IEnrollmentService _service;

    public EnrollmentsController(IEnrollmentService service)
    {
        _service = service;
    }

    // Id del usuario del token (claim sub, mapeado a NameIdentifier)
    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? "";

    // POST /api/enrollments  → inscribir al estudiante (del token) en un curso publicado
    [HttpPost]
    public async Task<IActionResult> Enroll([FromBody] EnrollmentRequest request)
    {
        var result = await _service.EnrollAsync(UserId, request);
        return result.Outcome switch
        {
            EnrollmentOutcome.CourseNotFound     => ApiError(404, "Curso no encontrado."),
            EnrollmentOutcome.CourseNotPublished => ApiError(400, "El curso no está publicado."),
            EnrollmentOutcome.AlreadyEnrolled    => ApiError(409, "Ya estás inscrito en este curso."),
            _                                    => StatusCode(StatusCodes.Status201Created, result.Enrollment)
        };
    }

    // GET /api/enrollments/me  → "mis cursos" (inscripciones del token)
    [HttpGet("me")]
    public async Task<IActionResult> GetMine()
        => Ok((await _service.GetMineAsync(UserId)).List);
}
