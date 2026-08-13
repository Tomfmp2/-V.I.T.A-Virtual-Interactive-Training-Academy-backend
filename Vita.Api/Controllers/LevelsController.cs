using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vita.Api.Dtos.Levels;
using Vita.Api.Services;

namespace Vita.Api.Controllers;

[ApiController]
[Route("api/levels")]
[Authorize]
public class LevelsController : BaseApiController
{
    private readonly ILevelService _service;

    public LevelsController(ILevelService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var level = await _service.GetByIdAsync(id);
        return level is null
            ? ApiError(404, "Nivel no encontrado.")
            : Ok(level);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] LevelRequest request)
    {
        var result = await _service.CreateAsync(request);
        return result.Outcome switch
        {
            LevelOutcome.NombreExists => ApiError(409, "Ya existe un nivel con ese nombre."),
            _ => StatusCode(StatusCodes.Status201Created, result.Level)
        };
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] LevelRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Outcome switch
        {
            LevelOutcome.NotFound => ApiError(404, "Nivel no encontrado."),
            LevelOutcome.NombreExists => ApiError(409, "Ya existe un nivel con ese nombre."),
            _ => Ok(result.Level)
        };
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Outcome switch
        {
            LevelOutcome.NotFound => ApiError(404, "Nivel no encontrado."),
            LevelOutcome.InUse => ApiError(409, "No se puede eliminar: el nivel está en uso por cursos."),
            _ => NoContent()
        };
    }
}
