using System.ComponentModel.DataAnnotations;

namespace TransportTrack.Infrastructure.Models;

public class ConductorModel
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Licencia { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Telefono { get; set; } = string.Empty;
}
