using System.ComponentModel.DataAnnotations;
using TransportTrack.Domain.Core;

namespace TransportTrack.Domain.Entities;

public class Vehiculo : BaseEntity
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

    public int Anio { get; set; }

    public int ConductorId { get; set; }

    public Conductor? Conductor { get; set; }
}
