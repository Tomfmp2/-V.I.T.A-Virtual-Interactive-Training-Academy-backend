using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Entities;

public class TipoLeccion
{
    public int IdTipoLeccion { get; set; }

    [MaxLength(50)]
    public string Nombre { get; set; } = string.Empty;

    public ICollection<Leccion> Lecciones { get; set; } = new List<Leccion>();
}
