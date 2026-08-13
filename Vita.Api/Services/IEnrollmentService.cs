using Vita.Api.Dtos.Enrollments;

namespace Vita.Api.Services;

public enum EnrollmentOutcome
{
    Success,
    CourseNotFound,
    CourseNotPublished,
    AlreadyEnrolled
}

public class EnrollmentResult
{
    public EnrollmentOutcome Outcome { get; set; }
    public EnrollmentResponse? Enrollment { get; set; }
    public List<EnrollmentResponse>? List { get; set; }
}

public interface IEnrollmentService
{
    Task<EnrollmentResult> EnrollAsync(string estudianteId, EnrollmentRequest request);
    Task<EnrollmentResult> GetMineAsync(string estudianteId);
}
