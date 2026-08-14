using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Entities;

public class Categoria
{
    public int IdCategoria { get; set; }

    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Descripcion { get; set; }

    [MaxLength(255)]
    public string? IconoUrl { get; set; }

    public bool Activo { get; set; } = true;

    public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
}
