using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Vita.Api.Dtos.Courses;
using Vita.Api.Services;

namespace Vita.Api.Controllers;

[ApiController]
[Route("api/courses")]
[Authorize]
public class CoursesController : BaseApiController
{
    private readonly ICourseService _service;
    private readonly JsonSerializerOptions _jsonOptions;

    public CoursesController(ICourseService service, IOptions<JsonOptions> jsonOptions)
    {
        _service = service;
        _jsonOptions = jsonOptions.Value.JsonSerializerOptions;
    }

    private string? CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

    private string CurrentRole =>
        User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role") ?? "Estudiante";

    private bool TryDeserializeAndValidate<T>(JsonElement body, out T? request)
        where T : class
    {
        ModelState.Clear();
        request = null;

        if (body.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            ModelState.AddModelError(string.Empty, "Datos inválidos.");
            return false;
        }

        try
        {
            request = body.Deserialize<T>(_jsonOptions);
        }
        catch (JsonException)
        {
            ModelState.AddModelError(string.Empty, "Datos inválidos.");
            return false;
        }
        catch (NotSupportedException)
        {
            ModelState.AddModelError(string.Empty, "Datos inválidos.");
            return false;
        }

        if (request is null)
        {
            ModelState.AddModelError(string.Empty, "Datos inválidos.");
            return false;
        }

        return TryValidateModel(request);
    }

    private static JsonElement RemoveInstructorId(JsonElement body)
    {
        if (body.ValueKind != JsonValueKind.Object)
            return body;

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();

            foreach (var property in body.EnumerateObject())
            {
                if (property.Name.Equals("idInstructor", StringComparison.OrdinalIgnoreCase))
                    continue;

                property.WriteTo(writer);
            }

            writer.WriteEndObject();
        }

        stream.Position = 0;
        using var document = JsonDocument.Parse(stream);
        return document.RootElement.Clone();
    }

    private IActionResult ModelValidationError()
    {
        var message = ModelState
            .Where(kv => kv.Value?.Errors.Count > 0)
            .SelectMany(kv => kv.Value!.Errors)
            .Select(error => error.ErrorMessage)
            .FirstOrDefault() ?? "Datos inválidos.";

        return ApiError(400, message);
    }

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

    // POST /api/courses — Instructor usa token; Admin indica un Instructor válido
    [HttpPost]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Create([FromBody] JsonElement body)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return ApiError(401, "No autorizado.");

        CourseResult result;
        if (CurrentRole == "Admin")
        {
            if (!TryDeserializeAndValidate(body, out CourseAdminCreateRequest? request) || request is null)
                return ModelValidationError();

            result = await _service.CreateForAdminAsync(request);
        }
        else
        {
            var instructorBody = RemoveInstructorId(body);
            if (!TryDeserializeAndValidate(instructorBody, out CourseCreateRequest? request) || request is null)
                return ModelValidationError();

            result = await _service.CreateAsync(request, userId);
        }

        return result.Outcome switch
        {
            CourseOutcome.InvalidInstructor => ApiError(400, "Debe asignar un instructor válido con rol Instructor."),
            CourseOutcome.CategoryNotFound => ApiError(400, "La categoría no existe o está inactiva."),
            CourseOutcome.NivelNotFound => ApiError(400, "El nivel no existe."),
            CourseOutcome.TituloExists => ApiError(409, "Ya tienes un curso con ese título."),
            _ => CreatedAtAction(nameof(GetById), new { id = result.Course!.Id }, result.Course)
        };
    }

    // PUT /api/courses/{id} — Instructor dueño o Admin
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CourseUpdateRequest request)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return ApiError(401, "No autorizado.");

        var result = await _service.UpdateAsync(id, request, userId, CurrentRole);
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

    // PATCH /api/courses/{id}/status — Instructor dueño o Admin
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Instructor,Admin")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] CourseStatusRequest request)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return ApiError(401, "No autorizado.");

        var result = await _service.ChangeStatusAsync(id, request, userId, CurrentRole);
        return result.Outcome switch
        {
            CourseOutcome.InvalidEstado => ApiError(400, "Estado inválido. Valores permitidos: borrador, publicado."),
            CourseOutcome.NoLessons => ApiError(400, "El curso debe tener al menos una lección antes de publicarse."),
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
