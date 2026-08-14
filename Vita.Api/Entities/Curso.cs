using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Entities;

public class Curso
{
    public int IdCurso { get; set; }

    [MaxLength(450)]
    public string IdInstructor { get; set; } = string.Empty;

    public int IdCategoria { get; set; }
    public int IdNivel { get; set; }
    public int IdEstadoCurso { get; set; }

    [MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [MaxLength(220)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? DescripcionCorta { get; set; }

    public string? DescripcionLarga { get; set; }

    [MaxLength(255)]
    public string? ImagenPortadaUrl { get; set; }

    public int? DuracionEstimadaMin { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdateAt { get; set; }

    public Usuario Instructor { get; set; } = null!;
    public Categoria Categoria { get; set; } = null!;
    public Nivel Nivel { get; set; } = null!;
    public EstadoCurso EstadoCurso { get; set; } = null!;

    public ICollection<Leccion> Lecciones { get; set; } = new List<Leccion>();
    public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
}
