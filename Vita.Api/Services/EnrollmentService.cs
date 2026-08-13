using Microsoft.EntityFrameworkCore;
using Vita.Api.Data;
using Vita.Api.Dtos.Enrollments;
using Vita.Api.Entities;

namespace Vita.Api.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly ApplicationDbContext _db;

    public EnrollmentService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<EnrollmentResult> EnrollAsync(string estudianteId, EnrollmentRequest request)
    {
        // 1) El curso debe existir
        var curso = await _db.Cursos
            .Include(c => c.EstadoCurso)
            .FirstOrDefaultAsync(c => c.IdCurso == request.CursoId);
        if (curso is null)
            return new EnrollmentResult { Outcome = EnrollmentOutcome.CourseNotFound };

        // 2) El curso debe estar PUBLICADO (no borrador)
        if (curso.EstadoCurso.Nombre != "Publicado")
            return new EnrollmentResult { Outcome = EnrollmentOutcome.CourseNotPublished };

        // 3) No inscribir dos veces (además del índice UNIQUE en BD)
        var yaInscrito = await _db.Inscripciones
            .AnyAsync(i => i.IdEstudiante == estudianteId && i.IdCurso == request.CursoId);
        if (yaInscrito)
            return new EnrollmentResult { Outcome = EnrollmentOutcome.AlreadyEnrolled };

        // 4) Estado "Activa" para la nueva inscripción
        var estadoActiva = await _db.EstadosInscripcion
            .Where(e => e.Nombre == "Activa")
            .Select(e => e.IdEstadoInscripcion)
            .FirstAsync();

        var inscripcion = new Inscripcion
        {
            IdEstudiante = estudianteId,   // SIEMPRE del token, nunca del body
            IdCurso = request.CursoId,
            IdEstadoInscripcion = estadoActiva,
            FechaInscripcion = DateTime.UtcNow
        };

        _db.Inscripciones.Add(inscripcion);
        await _db.SaveChangesAsync();

        return new EnrollmentResult
        {
            Outcome = EnrollmentOutcome.Success,
            Enrollment = new EnrollmentResponse
            {
                Id = inscripcion.IdInscripcion,
                CursoId = curso.IdCurso,
                CursoTitulo = curso.Titulo,
                FechaInscripcion = inscripcion.FechaInscripcion,
                Estado = "Activa"
            }
        };
    }

    public async Task<EnrollmentResult> GetMineAsync(string estudianteId)
    {
        var list = await _db.Inscripciones
            .Where(i => i.IdEstudiante == estudianteId)
            .OrderByDescending(i => i.FechaInscripcion)
            .Select(i => new EnrollmentResponse
            {
                Id = i.IdInscripcion,
                CursoId = i.IdCurso,
                CursoTitulo = i.Curso.Titulo,
                FechaInscripcion = i.FechaInscripcion,
                Estado = i.EstadoInscripcion.Nombre
            })
            .ToListAsync();

        return new EnrollmentResult { Outcome = EnrollmentOutcome.Success, List = list };
    }
}
