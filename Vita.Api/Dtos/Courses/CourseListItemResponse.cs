namespace Vita.Api.Dtos.Courses;

public class CourseListItemResponse
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? DescripcionCorta { get; set; }
    public string? ImagenPortadaUrl { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public string NivelNombre { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string InstructorNombre { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
