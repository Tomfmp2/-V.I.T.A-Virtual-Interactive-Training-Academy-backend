using Microsoft.EntityFrameworkCore;
using Vita.Api.Data;
using Vita.Api.Dtos.Lessons;
using Vita.Api.Entities;

namespace Vita.Api.Services;

public class LessonService : ILessonService
{
    private readonly ApplicationDbContext _db;
    private readonly ICourseOwnershipService _ownership;

    public LessonService(ApplicationDbContext db, ICourseOwnershipService ownership)
    {
        _db = db;
        _ownership = ownership;
    }

    public async Task<LessonResult> GetByCourseAsync(int courseId)
    {
        if (!await _db.Cursos.AnyAsync(c => c.IdCurso == courseId))
            return new LessonResult { Outcome = LessonOutcome.CourseNotFound };

        var lessons = await _db.Lecciones
            .Where(l => l.IdCurso == courseId)
            .OrderBy(l => l.Orden)
            .Select(l => new LessonResponse
            {
                Id = l.IdLeccion,
                CursoId = l.IdCurso,
                Titulo = l.Titulo,
                Descripcion = l.Contenido,
                Recurso = l.RecursoUrl,
                Orden = l.Orden
            })
            .ToListAsync();

        return new LessonResult { Outcome = LessonOutcome.Success, Lessons = lessons };
    }

    public async Task<LessonResult> GetByIdAsync(int courseId, int lessonId)
    {
        if (!await _db.Cursos.AnyAsync(c => c.IdCurso == courseId))
            return new LessonResult { Outcome = LessonOutcome.CourseNotFound };

        var leccion = await _db.Lecciones
            .FirstOrDefaultAsync(l => l.IdLeccion == lessonId && l.IdCurso == courseId);
        if (leccion is null)
            return new LessonResult { Outcome = LessonOutcome.LessonNotFound };

        return new LessonResult { Outcome = LessonOutcome.Success, Lesson = MapToResponse(leccion) };
    }

    public async Task<LessonResult> CreateAsync(int courseId, string userId, LessonRequest request)
    {
        if (!await _db.Cursos.AnyAsync(c => c.IdCurso == courseId))
            return new LessonResult { Outcome = LessonOutcome.CourseNotFound };

        // Solo el instructor dueño del curso puede escribir
        if (!await _ownership.IsCourseOwnerAsync(userId, courseId))
            return new LessonResult { Outcome = LessonOutcome.NotOwner };

        // La tarjeta no maneja "tipo": se usa el primer tipo del catálogo por defecto.
        var tipoDefault = await _db.TiposLeccion
            .OrderBy(t => t.IdTipoLeccion)
            .Select(t => t.IdTipoLeccion)
            .FirstAsync();

        var leccion = new Leccion
        {
            IdCurso = courseId,
            IdTipoLeccion = tipoDefault,
            Titulo = request.Titulo.Trim(),
            Contenido = request.Descripcion,
            RecursoUrl = request.Recurso,
            Orden = request.Orden
        };

        _db.Lecciones.Add(leccion);
        await _db.SaveChangesAsync();

        return new LessonResult { Outcome = LessonOutcome.Success, Lesson = MapToResponse(leccion) };
    }

    public async Task<LessonResult> UpdateAsync(int courseId, int lessonId, string userId, LessonRequest request)
    {
        if (!await _db.Cursos.AnyAsync(c => c.IdCurso == courseId))
            return new LessonResult { Outcome = LessonOutcome.CourseNotFound };

        if (!await _ownership.IsCourseOwnerAsync(userId, courseId))
            return new LessonResult { Outcome = LessonOutcome.NotOwner };

        var leccion = await _db.Lecciones
            .FirstOrDefaultAsync(l => l.IdLeccion == lessonId && l.IdCurso == courseId);
        if (leccion is null)
            return new LessonResult { Outcome = LessonOutcome.LessonNotFound };

        leccion.Titulo = request.Titulo.Trim();
        leccion.Contenido = request.Descripcion;
        leccion.RecursoUrl = request.Recurso;
        leccion.Orden = request.Orden;
        await _db.SaveChangesAsync();

        return new LessonResult { Outcome = LessonOutcome.Success, Lesson = MapToResponse(leccion) };
    }

    public async Task<LessonResult> DeleteAsync(int courseId, int lessonId, string userId)
    {
        if (!await _db.Cursos.AnyAsync(c => c.IdCurso == courseId))
            return new LessonResult { Outcome = LessonOutcome.CourseNotFound };

        if (!await _ownership.IsCourseOwnerAsync(userId, courseId))
            return new LessonResult { Outcome = LessonOutcome.NotOwner };

        var leccion = await _db.Lecciones
            .FirstOrDefaultAsync(l => l.IdLeccion == lessonId && l.IdCurso == courseId);
        if (leccion is null)
            return new LessonResult { Outcome = LessonOutcome.LessonNotFound };

        _db.Lecciones.Remove(leccion);
        await _db.SaveChangesAsync();

        return new LessonResult { Outcome = LessonOutcome.Success };
    }

    private static LessonResponse MapToResponse(Leccion l) => new()
    {
        Id = l.IdLeccion,
        CursoId = l.IdCurso,
        Titulo = l.Titulo,
        Descripcion = l.Contenido,
        Recurso = l.RecursoUrl,
        Orden = l.Orden
    };
}
