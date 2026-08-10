using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Entities;

public class Leccion
{
    public int IdLeccion { get; set; }

    public int IdCurso { get; set; }
    public int IdTipoLeccion { get; set; }

    [MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    public string? Resumen { get; set; }
    public string? Contenido { get; set; }

    [MaxLength(255)]
    public string? RecursoUrl { get; set; }

    public int Orden { get; set; } = 1;
    public int? DuracionMin { get; set; }

    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

    public Curso Curso { get; set; } = null!;
    public TipoLeccion TipoLeccion { get; set; } = null!;
}
