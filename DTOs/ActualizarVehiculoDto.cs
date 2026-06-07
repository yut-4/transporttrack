using System.ComponentModel.DataAnnotations;

namespace transporttrack.DTOs;

public class ActualizarVehiculoDto
{
    [Required]
    [MaxLength(20)]
    public string Placa { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    public string Marca { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    public string Modelo { get; set; } = string.Empty;

    [Range(1900, 2100)]
    public int Anio { get; set; }

    [Range(1, int.MaxValue)]
    public int ConductorId { get; set; }
}
