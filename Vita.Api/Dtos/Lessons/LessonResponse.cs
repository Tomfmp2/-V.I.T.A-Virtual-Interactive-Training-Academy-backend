namespace Vita.Api.Dtos.Lessons;

public class LessonResponse
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Recurso { get; set; }
    public int Orden { get; set; }
}
