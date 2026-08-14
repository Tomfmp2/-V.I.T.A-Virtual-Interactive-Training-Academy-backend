using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Entities;

public class EstadoInscripcion
{
    public int IdEstadoInscripcion { get; set; }

    [MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Inscripcion> Inscripciones { get; set; } = new List<Inscripcion>();
}
