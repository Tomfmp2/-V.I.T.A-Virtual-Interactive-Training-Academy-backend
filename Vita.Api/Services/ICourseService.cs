using Vita.Api.Dtos.Courses;

namespace Vita.Api.Services;

public enum CourseOutcome
{
    Success, NotFound, Forbidden, CategoryNotFound, NivelNotFound,
    TituloExists, InvalidEstado, HasEnrollments, HasLessons
}

public class CourseResult
{
    public CourseOutcome Outcome { get; set; }
    public CourseResponse? Course { get; set; }
}

public interface ICourseService
{
    Task<List<CourseListItemResponse>> GetAllAsync(string userId, string role);
    Task<CourseResponse?> GetByIdAsync(int id, string userId, string role);
    Task<List<CourseListItemResponse>> GetMineAsync(string userId);
    Task<CourseResult> CreateAsync(CourseCreateRequest request, string instructorId);
    Task<CourseResult> UpdateAsync(int id, CourseUpdateRequest request, string userId);
    Task<CourseResult> ChangeStatusAsync(int id, CourseStatusRequest request, string userId);
    Task<CourseResult> DeleteAsync(int id, string userId, string role);
}
