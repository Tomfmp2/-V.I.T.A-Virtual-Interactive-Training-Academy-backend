using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vita.Api.Services;

namespace Vita.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : BaseApiController
{
    private readonly IReportService _service;

    public ReportsController(IReportService service)
    {
        _service = service;
    }

    private string UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? "";

    private string CurrentRole =>
        User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role") ?? "";

    /// <summary>Cantidad de cursos agrupados por instructor. Solo Admin.</summary>
    [HttpGet("courses-by-instructor")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetCoursesByInstructor([FromQuery] string? instructorId)
        => Ok(await _service.GetCoursesByInstructorAsync(instructorId));

    /// <summary>
    /// Estudiantes inscritos (estado Activa) por curso.
    /// Admin ve todos; Instructor solo los suyos.
    /// </summary>
    [HttpGet("students-by-course")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> GetStudentsByCourse()
        => Ok(await _service.GetStudentsByCourseAsync(UserId, CurrentRole));

    /// <summary>Ranking de cursos con más inscritos activos. Solo Admin.</summary>
    [HttpGet("top-courses")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetTopCourses([FromQuery] int limit = 10)
    {
        var result = await _service.GetTopCoursesAsync(limit);
        return result.Outcome switch
        {
            ReportOutcome.InvalidLimit => ApiError(400, "El parámetro limit debe estar entre 1 y 50."),
            _ => Ok(result.Items)
        };
    }
}
