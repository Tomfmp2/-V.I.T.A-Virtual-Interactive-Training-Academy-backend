using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Entities;

public class Nivel
{
    public int IdNivel { get; set; }

    [MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
}
