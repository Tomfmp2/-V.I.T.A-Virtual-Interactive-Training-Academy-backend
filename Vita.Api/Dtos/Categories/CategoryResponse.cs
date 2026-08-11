namespace Vita.Api.Dtos.Categories;

public class CategoryResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? IconoUrl { get; set; }
    public bool Activo { get; set; }
}
