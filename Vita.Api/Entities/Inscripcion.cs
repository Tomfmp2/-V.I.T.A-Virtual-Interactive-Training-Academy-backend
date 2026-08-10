using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Entities;

public class Inscripcion
{
    public int IdInscripcion { get; set; }

    [MaxLength(450)]
    public string IdEstudiante { get; set; } = string.Empty;

    public int IdCurso { get; set; }
    public int IdEstadoInscripcion { get; set; }

    public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;

    public Usuario Estudiante { get; set; } = null!;
    public Curso Curso { get; set; } = null!;
    public EstadoInscripcion EstadoInscripcion { get; set; } = null!;
}
