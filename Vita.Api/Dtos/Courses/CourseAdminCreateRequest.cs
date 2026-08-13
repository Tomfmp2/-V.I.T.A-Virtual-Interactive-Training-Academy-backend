using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Courses;

public class CourseAdminCreateRequest : CourseCreateRequest
{
    [Required]
    public string IdInstructor { get; set; } = string.Empty;
}
