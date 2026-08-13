using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Enrollments;

public class EnrollmentRequest
{
    [Range(1, int.MaxValue)]   // cursoId válido (>= 1)
    public int CursoId { get; set; }
}
