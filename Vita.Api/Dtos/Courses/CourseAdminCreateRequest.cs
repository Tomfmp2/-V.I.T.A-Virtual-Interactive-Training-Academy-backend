using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Courses;

public class CourseAdminCreateRequest : CourseCreateRequest
{
    [Required(ErrorMessage = "El instructor es obligatorio.")]
    public string IdInstructor { get; set; } = string.Empty;
}
