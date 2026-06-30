using System.ComponentModel.DataAnnotations;
using TransportTrack.Domain.Core;

namespace TransportTrack.Domain.Entities;

public class Conductor : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Licencia { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Telefono { get; set; } = string.Empty;

    public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
