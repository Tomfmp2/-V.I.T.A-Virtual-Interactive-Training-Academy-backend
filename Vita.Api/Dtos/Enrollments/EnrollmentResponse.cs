namespace Vita.Api.Dtos.Enrollments;

public class EnrollmentResponse
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public string CursoTitulo { get; set; } = string.Empty;
    public DateTime FechaInscripcion { get; set; }
    public string Estado { get; set; } = string.Empty;
}
