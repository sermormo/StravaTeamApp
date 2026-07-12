using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace StravaTeamApp.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    public string Genero { get; set; } = string.Empty; 

    [Required]
    public string UPIN { get; set; } = string.Empty;
}