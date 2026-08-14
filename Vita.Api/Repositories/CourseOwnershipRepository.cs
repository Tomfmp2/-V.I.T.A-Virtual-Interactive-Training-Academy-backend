using Microsoft.EntityFrameworkCore;
using Vita.Api.Data;

namespace Vita.Api.Repositories;

public class CourseOwnershipRepository : ICourseOwnershipRepository
{
    private readonly ApplicationDbContext _db;

    public CourseOwnershipRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<bool> ExistsCourseOwnedByAsync(string userId, int courseId)
    {
        return _db.Cursos
            .AsNoTracking()
            .AnyAsync(c => c.IdCurso == courseId && c.IdInstructor == userId);
    }
}
