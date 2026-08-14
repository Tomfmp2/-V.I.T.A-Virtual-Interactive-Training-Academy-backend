namespace Vita.Api.Dtos.Reports;

public class TopCourseItemResponse
{
    public int CursoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Instructor { get; set; } = string.Empty;
    public int TotalInscritos { get; set; }
}
