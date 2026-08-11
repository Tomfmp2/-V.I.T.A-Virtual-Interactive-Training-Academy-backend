using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vita.Api.Dtos.Categories;
using Vita.Api.Services;

namespace Vita.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize] // todos los endpoints requieren autenticación (nada público)
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    // GET /api/categories — cualquier usuario autenticado
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    // GET /api/categories/{id} — cualquier usuario autenticado
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var categoria = await _service.GetByIdAsync(id);
        return categoria is null
            ? ApiError(404, "Categoría no encontrada.")
            : Ok(categoria);
    }

    // POST /api/categories — solo Admin
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CategoryRequest request)
    {
        var result = await _service.CreateAsync(request);
        return result.Outcome switch
        {
            CategoryOutcome.NombreExists => ApiError(409, "Ya existe una categoría con ese nombre."),
            _ => StatusCode(StatusCodes.Status201Created, result.Category)
        };
    }

    // PUT /api/categories/{id} — solo Admin
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return result.Outcome switch
        {
            CategoryOutcome.NotFound => ApiError(404, "Categoría no encontrada."),
            CategoryOutcome.NombreExists => ApiError(409, "Ya existe una categoría con ese nombre."),
            _ => Ok(result.Category)
        };
    }

    // DELETE /api/categories/{id} — solo Admin; 409 si está en uso por cursos
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Outcome switch
        {
            CategoryOutcome.NotFound => ApiError(404, "Categoría no encontrada."),
            CategoryOutcome.InUse => ApiError(409, "No se puede eliminar: la categoría está en uso por cursos."),
            _ => NoContent()
        };
    }
}
