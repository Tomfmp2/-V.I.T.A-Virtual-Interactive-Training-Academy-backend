using Vita.Api.Dtos.Lessons;

namespace Vita.Api.Services;

public enum LessonOutcome
{
    Success,
    CourseNotFound,
    LessonNotFound,
    NotOwner
}

public class LessonResult
{
    public LessonOutcome Outcome { get; set; }
    public LessonResponse? Lesson { get; set; }
    public List<LessonResponse>? Lessons { get; set; }
}

public interface ILessonService
{
    Task<LessonResult> GetByCourseAsync(int courseId);
    Task<LessonResult> GetByIdAsync(int courseId, int lessonId);
    Task<LessonResult> CreateAsync(int courseId, string userId, string role, LessonRequest request);
    Task<LessonResult> UpdateAsync(int courseId, int lessonId, string userId, string role, LessonRequest request);
    Task<LessonResult> DeleteAsync(int courseId, int lessonId, string userId, string role);
}
