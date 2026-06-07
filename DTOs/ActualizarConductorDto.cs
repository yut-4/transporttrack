using System.ComponentModel.DataAnnotations;

namespace transporttrack.DTOs;

public class ActualizarConductorDto
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
