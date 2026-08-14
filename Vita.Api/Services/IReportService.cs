using Vita.Api.Dtos.Reports;

namespace Vita.Api.Services;

public enum ReportOutcome
{
    Success,
    InvalidLimit
}

public class ReportListResult<T>
{
    public ReportOutcome Outcome { get; set; }
    public List<T> Items { get; set; } = new();
}

public interface IReportService
{
    Task<List<CoursesByInstructorItemResponse>> GetCoursesByInstructorAsync(string? instructorId);

    Task<List<StudentsByCourseItemResponse>> GetStudentsByCourseAsync(string userId, string role);

    Task<ReportListResult<TopCourseItemResponse>> GetTopCoursesAsync(int limit);
}
