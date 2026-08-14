using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Courses;

public class CourseStatusRequest
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public string Estado { get; set; } = string.Empty;
}
