using System.ComponentModel.DataAnnotations;

namespace Vita.Api.Dtos.Courses;

public class CourseStatusRequest
{
    [Required]
    public string Estado { get; set; } = string.Empty;
}
