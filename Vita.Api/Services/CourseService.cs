using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vita.Api.Common;
using Vita.Api.Data;
using Vita.Api.Dtos.Courses;
using Vita.Api.Entities;

namespace Vita.Api.Services;

public class CourseService : ICourseService
{
    private readonly ApplicationDbContext _db;
    private readonly ICourseOwnershipService _ownership;
    private readonly UserManager<Usuario> _userManager;

    public CourseService(
        ApplicationDbContext db,
        ICourseOwnershipService ownership,
        UserManager<Usuario> userManager)
    {
        _db = db;
        _ownership = ownership;
        _userManager = userManager;
    }

    public async Task<List<CourseListItemResponse>> GetAllAsync(string userId, string role)
    {
        var query = _db.Cursos
            .AsNoTracking()
            .Include(c => c.Categoria)
            .Include(c => c.Nivel)
            .Include(c => c.EstadoCurso)
            .Include(c => c.Instructor)
            .AsQueryable();

        if (role == "Admin")
        {
            // sin filtro
        }
        else if (role == "Instructor")
        {
            // Tarjeta: Instructor solo ve los suyos (borrador + publicado)
            query = query.Where(c => c.IdInstructor == userId);
        }
        else
        {
            query = query.Where(c => c.EstadoCurso.Nombre == "Publicado");
        }

        var cursos = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return cursos.Select(MapToListItem).ToList();
    }

    public async Task<CourseResponse?> GetByIdAsync(int id, string userId, string role)
    {
        var curso = await _db.Cursos
            .AsNoTracking()
            .Include(c => c.Categoria)
            .Include(c => c.Nivel)
            .Include(c => c.EstadoCurso)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.IdCurso == id);

        if (curso is null)
            return null;

        var visible = role switch
        {
            "Admin" => true,
            "Instructor" => curso.IdInstructor == userId,
            _ => curso.EstadoCurso.Nombre == "Publicado"
        };

        // Si existe pero no es visible → null (404, no 403) para no filtrar existencia
        return visible ? MapToResponse(curso) : null;
    }

    public async Task<List<CourseListItemResponse>> GetMineAsync(string userId)
    {
        var cursos = await _db.Cursos
            .AsNoTracking()
            .Include(c => c.Categoria)
            .Include(c => c.Nivel)
            .Include(c => c.EstadoCurso)
            .Include(c => c.Instructor)
            .Where(c => c.IdInstructor == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return cursos.Select(MapToListItem).ToList();
    }

    public async Task<CourseResult> CreateAsync(CourseCreateRequest request, string instructorId)
    {
        var categoriaOk = await _db.Categorias
            .AnyAsync(c => c.IdCategoria == request.IdCategoria && c.Activo);
        if (!categoriaOk)
            return new CourseResult { Outcome = CourseOutcome.CategoryNotFound };

        var nivelOk = await _db.Niveles.AnyAsync(n => n.IdNivel == request.IdNivel);
        if (!nivelOk)
            return new CourseResult { Outcome = CourseOutcome.NivelNotFound };

        var titulo = request.Titulo.Trim();
        var tituloExiste = await _db.Cursos.AnyAsync(c =>
            c.IdInstructor == instructorId && c.Titulo.ToLower() == titulo.ToLower());
        if (tituloExiste)
            return new CourseResult { Outcome = CourseOutcome.TituloExists };

        var curso = new Curso
        {
            Titulo = titulo,
            Slug = await GenerateUniqueSlugAsync(titulo),
            IdCategoria = request.IdCategoria,
            IdNivel = request.IdNivel,
            DescripcionCorta = request.DescripcionCorta,
            DescripcionLarga = request.DescripcionLarga,
            ImagenPortadaUrl = request.ImagenPortadaUrl,
            DuracionEstimadaMin = request.DuracionEstimadaMin,
            IdInstructor = instructorId,
            IdEstadoCurso = await GetEstadoIdAsync("Borrador"),
            CreatedAt = DateTime.UtcNow
        };

        _db.Cursos.Add(curso);
        await _db.SaveChangesAsync();

        var created = await LoadCourseAsync(curso.IdCurso);
        return new CourseResult { Outcome = CourseOutcome.Success, Course = MapToResponse(created!) };
    }

    public async Task<CourseResult> CreateForAdminAsync(CourseAdminCreateRequest request)
    {
        var instructorId = request.IdInstructor?.Trim();
        if (string.IsNullOrWhiteSpace(instructorId))
            return new CourseResult { Outcome = CourseOutcome.InvalidInstructor };

        var instructor = await _userManager.FindByIdAsync(instructorId);
        if (instructor is null || !await _userManager.IsInRoleAsync(instructor, "Instructor"))
            return new CourseResult { Outcome = CourseOutcome.InvalidInstructor };

        return await CreateAsync(request, instructorId);
    }

    public async Task<CourseResult> UpdateAsync(
        int id,
        CourseUpdateRequest request,
        string userId,
        string role)
    {
        var curso = await _db.Cursos.FindAsync(id);
        if (curso is null)
            return new CourseResult { Outcome = CourseOutcome.NotFound };

        var autorizado = role == "Admin" || await _ownership.IsCourseOwnerAsync(userId, id);
        if (!autorizado)
            return new CourseResult { Outcome = CourseOutcome.Forbidden };

        var categoriaOk = await _db.Categorias
            .AnyAsync(c => c.IdCategoria == request.IdCategoria && c.Activo);
        if (!categoriaOk)
            return new CourseResult { Outcome = CourseOutcome.CategoryNotFound };

        var nivelOk = await _db.Niveles.AnyAsync(n => n.IdNivel == request.IdNivel);
        if (!nivelOk)
            return new CourseResult { Outcome = CourseOutcome.NivelNotFound };

        var titulo = request.Titulo.Trim();
        var tituloExiste = await _db.Cursos.AnyAsync(c =>
            c.IdCurso != id
            && c.IdInstructor == curso.IdInstructor
            && c.Titulo.ToLower() == titulo.ToLower());
        if (tituloExiste)
            return new CourseResult { Outcome = CourseOutcome.TituloExists };

        if (!string.Equals(curso.Titulo, titulo, StringComparison.Ordinal))
            curso.Slug = await GenerateUniqueSlugAsync(titulo, id);

        curso.Titulo = titulo;
        curso.IdCategoria = request.IdCategoria;
        curso.IdNivel = request.IdNivel;
        curso.DescripcionCorta = request.DescripcionCorta;
        curso.DescripcionLarga = request.DescripcionLarga;
        curso.ImagenPortadaUrl = request.ImagenPortadaUrl;
        curso.DuracionEstimadaMin = request.DuracionEstimadaMin;
        curso.UpdateAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var updated = await LoadCourseAsync(id);
        return new CourseResult { Outcome = CourseOutcome.Success, Course = MapToResponse(updated!) };
    }

    public async Task<CourseResult> ChangeStatusAsync(
        int id,
        CourseStatusRequest request,
        string userId,
        string role)
    {
        var estado = request.Estado.Trim().ToLowerInvariant();
        if (estado is not ("borrador" or "publicado"))
            return new CourseResult { Outcome = CourseOutcome.InvalidEstado };

        var curso = await _db.Cursos.FindAsync(id);
        if (curso is null)
            return new CourseResult { Outcome = CourseOutcome.NotFound };

        var autorizado = role == "Admin" || await _ownership.IsCourseOwnerAsync(userId, id);
        if (!autorizado)
            return new CourseResult { Outcome = CourseOutcome.Forbidden };

        var nombreEstado = estado == "borrador" ? "Borrador" : "Publicado";
        curso.IdEstadoCurso = await GetEstadoIdAsync(nombreEstado);
        curso.UpdateAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var updated = await LoadCourseAsync(id);
        return new CourseResult { Outcome = CourseOutcome.Success, Course = MapToResponse(updated!) };
    }

    public async Task<CourseResult> DeleteAsync(int id, string userId, string role)
    {
        var curso = await _db.Cursos.FindAsync(id);
        if (curso is null)
            return new CourseResult { Outcome = CourseOutcome.NotFound };

        var autorizado = role == "Admin" || await _ownership.IsCourseOwnerAsync(userId, id);
        if (!autorizado)
            return new CourseResult { Outcome = CourseOutcome.Forbidden };

        var tieneInscripciones = await _db.Inscripciones.AnyAsync(i => i.IdCurso == id);
        if (tieneInscripciones)
            return new CourseResult { Outcome = CourseOutcome.HasEnrollments };

        var tieneLecciones = await _db.Lecciones.AnyAsync(l => l.IdCurso == id);
        if (tieneLecciones)
            return new CourseResult { Outcome = CourseOutcome.HasLessons };

        _db.Cursos.Remove(curso);
        await _db.SaveChangesAsync();

        return new CourseResult { Outcome = CourseOutcome.Success };
    }

    private async Task<int> GetEstadoIdAsync(string nombre) =>
        await _db.EstadosCurso.Where(e => e.Nombre == nombre)
                              .Select(e => e.IdEstadoCurso)
                              .FirstAsync();

    private async Task<string> GenerateUniqueSlugAsync(string titulo, int? excludeCourseId = null)
    {
        var baseSlug = SlugHelper.Generate(titulo);
        var slug = baseSlug;
        var suffix = 2;

        while (await _db.Cursos.AnyAsync(c =>
                   c.Slug == slug
                   && (excludeCourseId == null || c.IdCurso != excludeCourseId.Value)))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private async Task<Curso?> LoadCourseAsync(int id) =>
        await _db.Cursos
            .AsNoTracking()
            .Include(c => c.Categoria)
            .Include(c => c.Nivel)
            .Include(c => c.EstadoCurso)
            .Include(c => c.Instructor)
            .FirstOrDefaultAsync(c => c.IdCurso == id);

    private static CourseListItemResponse MapToListItem(Curso c) => new()
    {
        Id = c.IdCurso,
        Titulo = c.Titulo,
        Slug = c.Slug,
        DescripcionCorta = c.DescripcionCorta,
        ImagenPortadaUrl = c.ImagenPortadaUrl,
        CategoriaNombre = c.Categoria.Nombre,
        NivelNombre = c.Nivel.Nombre,
        Estado = c.EstadoCurso.Nombre,
        InstructorNombre = $"{c.Instructor.Nombre} {c.Instructor.Apellido}",
        CreatedAt = c.CreatedAt
    };

    private static CourseResponse MapToResponse(Curso c) => new()
    {
        Id = c.IdCurso,
        Titulo = c.Titulo,
        Slug = c.Slug,
        DescripcionCorta = c.DescripcionCorta,
        DescripcionLarga = c.DescripcionLarga,
        ImagenPortadaUrl = c.ImagenPortadaUrl,
        DuracionEstimadaMin = c.DuracionEstimadaMin,
        IdCategoria = c.IdCategoria,
        CategoriaNombre = c.Categoria.Nombre,
        IdNivel = c.IdNivel,
        NivelNombre = c.Nivel.Nombre,
        Estado = c.EstadoCurso.Nombre,
        IdInstructor = c.IdInstructor,
        InstructorNombre = $"{c.Instructor.Nombre} {c.Instructor.Apellido}",
        CreatedAt = c.CreatedAt,
        UpdateAt = c.UpdateAt
    };
}
