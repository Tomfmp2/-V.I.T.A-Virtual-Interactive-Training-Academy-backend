using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Entities;

public class EstadoCurso
{
    public int IdEstadoCurso { get; set; }

    [MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
}
