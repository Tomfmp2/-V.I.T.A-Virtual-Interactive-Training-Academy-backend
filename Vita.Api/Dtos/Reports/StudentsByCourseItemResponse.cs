namespace Vita.Api.Dtos.Reports;

public class StudentsByCourseItemResponse
{
    public int CursoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int TotalEstudiantes { get; set; }
}
