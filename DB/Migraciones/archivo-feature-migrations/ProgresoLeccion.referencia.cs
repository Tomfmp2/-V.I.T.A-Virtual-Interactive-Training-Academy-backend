namespace Vita.Api.Entities;

public class ProgresoLeccion
{
    public int IdProgreso { get; set; }

    public string IdEstudiante { get; set; } = null!;
    public int IdLeccion { get; set; }

    public bool Completada { get; set; } = true;
    public DateTime VistoEn { get; set; }

    public ApplicationUser Estudiante { get; set; } = null!;
    public Leccion Leccion { get; set; } = null!;
}
