using System.ComponentModel.DataAnnotations;

namespace StravaTeamApp.Models;

public class SystemLog
{
    [Key]
    public int Id { get; set; }
    
    public DateTime Fecha { get; set; } = DateTime.Now;
    
    [Required]
    [MaxLength(50)]
    public string Nivel { get; set; } = string.Empty; // Ej: "Error", "Warning", "Info"
    
    [Required]
    public string Mensaje { get; set; } = string.Empty;
    
    public string Detalles { get; set; } = string.Empty;
}