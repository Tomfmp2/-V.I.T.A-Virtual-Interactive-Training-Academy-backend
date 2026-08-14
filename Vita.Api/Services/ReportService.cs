using Microsoft.EntityFrameworkCore;
using Vita.Api.Data;
using Vita.Api.Dtos.Reports;

namespace Vita.Api.Services;

public class ReportService : IReportService
{
    private const string EstadoInscripcionActiva = "Activa";
    private const int DefaultTopLimit = 10;
    private const int MinTopLimit = 1;
    private const int MaxTopLimit = 50;

    private readonly ApplicationDbContext _db;

    public ReportService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<CoursesByInstructorItemResponse>> GetCoursesByInstructorAsync(string? instructorId)
    {
        var query = _db.Cursos
            .AsNoTracking()
            .Include(c => c.Instructor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(instructorId))
            query = query.Where(c => c.IdInstructor == instructorId.Trim());

        var grupos = await query
            .GroupBy(c => new
            {
                c.IdInstructor,
                c.Instructor.Nombre,
                c.Instructor.Apellido
            })
            .Select(g => new CoursesByInstructorItemResponse
            {
                InstructorId = g.Key.IdInstructor,
                Instructor = ((g.Key.Nombre ?? "") + " " + (g.Key.Apellido ?? "")).Trim(),
                TotalCursos = g.Count()
            })
            .OrderByDescending(x => x.TotalCursos)
            .ThenBy(x => x.Instructor)
            .ToListAsync();

        return grupos;
    }

    public async Task<List<StudentsByCourseItemResponse>> GetStudentsByCourseAsync(string userId, string role)
    {
        var cursosQuery = _db.Cursos.AsNoTracking().AsQueryable();

        if (role == "Instructor")
            cursosQuery = cursosQuery.Where(c => c.IdInstructor == userId);

        var list = await cursosQuery
            .Select(c => new StudentsByCourseItemResponse
            {
                CursoId = c.IdCurso,
                Titulo = c.Titulo,
                TotalEstudiantes = c.Inscripciones
                    .Count(i => i.EstadoInscripcion.Nombre == EstadoInscripcionActiva)
            })
            .OrderByDescending(x => x.TotalEstudiantes)
            .ThenBy(x => x.Titulo)
            .ToListAsync();

        return list;
    }

    public async Task<ReportListResult<TopCourseItemResponse>> GetTopCoursesAsync(int limit)
    {
        if (limit < MinTopLimit || limit > MaxTopLimit)
            return new ReportListResult<TopCourseItemResponse> { Outcome = ReportOutcome.InvalidLimit };

        var effectiveLimit = limit <= 0 ? DefaultTopLimit : limit;

        var list = await _db.Cursos
            .AsNoTracking()
            .Select(c => new TopCourseItemResponse
            {
                CursoId = c.IdCurso,
                Titulo = c.Titulo,
                Instructor = ((c.Instructor.Nombre ?? "") + " " + (c.Instructor.Apellido ?? "")).Trim(),
                TotalInscritos = c.Inscripciones
                    .Count(i => i.EstadoInscripcion.Nombre == EstadoInscripcionActiva)
            })
            .OrderByDescending(x => x.TotalInscritos)
            .ThenBy(x => x.CursoId)
            .Take(effectiveLimit)
            .ToListAsync();

        return new ReportListResult<TopCourseItemResponse>
        {
            Outcome = ReportOutcome.Success,
            Items = list
        };
    }
}
