using Vita.Api.Dtos.Courses;

namespace Vita.Api.Services;

public enum CourseOutcome
{
    Success, NotFound, Forbidden, CategoryNotFound, NivelNotFound,
    TituloExists, InvalidEstado, InvalidInstructor, HasEnrollments, HasLessons, NoLessons,
    AdminCannotPublish
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
    Task<CourseResult> CreateForAdminAsync(CourseAdminCreateRequest request);
    Task<CourseResult> UpdateAsync(int id, CourseUpdateRequest request, string userId, string role);
    Task<CourseResult> ChangeStatusAsync(int id, CourseStatusRequest request, string userId, string role);
    Task<CourseResult> DeleteAsync(int id, string userId, string role);
}
