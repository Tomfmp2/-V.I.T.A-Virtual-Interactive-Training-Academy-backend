namespace Vita.Api.Dtos.Reports;

public class CoursesByInstructorItemResponse
{
    public string InstructorId { get; set; } = string.Empty;
    public string Instructor { get; set; } = string.Empty;
    public int TotalCursos { get; set; }
}
