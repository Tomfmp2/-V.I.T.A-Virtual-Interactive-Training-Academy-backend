using Microsoft.EntityFrameworkCore;
using Vita.Api.Common;
using Vita.Api.Data;
using Vita.Api.Dtos.Categories;
using Vita.Api.Entities;

namespace Vita.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _db;

    public CategoryService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoryResponse>> GetAllAsync()
    {
        return await _db.Categorias
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoryResponse
            {
                Id = c.IdCategoria,
                Nombre = c.Nombre,
                Slug = c.Slug,
                Descripcion = c.Descripcion,
                IconoUrl = c.IconoUrl,
                Activo = c.Activo
            })
            .ToListAsync();
    }

    public async Task<CategoryResponse?> GetByIdAsync(int id)
    {
        var categoria = await _db.Categorias.FindAsync(id);
        return categoria is null ? null : MapToResponse(categoria);
    }

    public async Task<CategoryResult> CreateAsync(CategoryRequest request)
    {
        var nombre = request.Nombre.Trim();
        var slug = SlugHelper.Generate(nombre);

        // Nombre único (case-insensitive) y slug único (evita choque por tildes/mayúsculas)
        var existe = await _db.Categorias.AnyAsync(c => c.Nombre.ToLower() == nombre.ToLower() || c.Slug == slug);
        if (existe)
            return new CategoryResult { Outcome = CategoryOutcome.NombreExists };

        var categoria = new Categoria
        {
            Nombre = nombre,
            Slug = slug,
            Descripcion = request.Descripcion,
            IconoUrl = request.IconoUrl,
            Activo = true
        };

        _db.Categorias.Add(categoria);
        await _db.SaveChangesAsync();

        return new CategoryResult { Outcome = CategoryOutcome.Success, Category = MapToResponse(categoria) };
    }

    public async Task<CategoryResult> UpdateAsync(int id, CategoryRequest request)
    {
        var categoria = await _db.Categorias.FindAsync(id);
        if (categoria is null)
            return new CategoryResult { Outcome = CategoryOutcome.NotFound };

        var nombre = request.Nombre.Trim();
        var slug = SlugHelper.Generate(nombre);

        // Nombre y slug únicos, excluyendo la propia categoría
        var existe = await _db.Categorias
            .AnyAsync(c => c.IdCategoria != id && (c.Nombre.ToLower() == nombre.ToLower() || c.Slug == slug));
        if (existe)
            return new CategoryResult { Outcome = CategoryOutcome.NombreExists };

        categoria.Nombre = nombre;
        categoria.Slug = slug;
        categoria.Descripcion = request.Descripcion;
        categoria.IconoUrl = request.IconoUrl;
        await _db.SaveChangesAsync();

        return new CategoryResult { Outcome = CategoryOutcome.Success, Category = MapToResponse(categoria) };
    }

    public async Task<CategoryResult> DeleteAsync(int id)
    {
        var categoria = await _db.Categorias.FindAsync(id);
        if (categoria is null)
            return new CategoryResult { Outcome = CategoryOutcome.NotFound };

        // Regla de negocio: si hay cursos con esta categoría, NO se puede borrar → 409
        var enUso = await _db.Cursos.AnyAsync(c => c.IdCategoria == id);
        if (enUso)
            return new CategoryResult { Outcome = CategoryOutcome.InUse };

        _db.Categorias.Remove(categoria);
        await _db.SaveChangesAsync();

        return new CategoryResult { Outcome = CategoryOutcome.Success };
    }

    private static CategoryResponse MapToResponse(Categoria c) => new()
    {
        Id = c.IdCategoria,
        Nombre = c.Nombre,
        Slug = c.Slug,
        Descripcion = c.Descripcion,
        IconoUrl = c.IconoUrl,
        Activo = c.Activo
    };
}
