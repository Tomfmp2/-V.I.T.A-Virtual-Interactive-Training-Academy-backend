using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Vita.Api.Entities;

public class Usuario : IdentityUser
{
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;
    [MaxLength(100)]
    public string Apellido { get; set; } =string.Empty;
    [MaxLength(255)]
    public string? FotoUrl { get; set; }
    public string? Biografia { get; set; } 
    public bool Activo { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
}