using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Enrollments;

public class EnrollmentRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "El curso no es válido.")]
    public int CursoId { get; set; }
}
