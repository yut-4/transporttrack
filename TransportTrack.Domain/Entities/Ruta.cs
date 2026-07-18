using System.ComponentModel.DataAnnotations;
using TransportTrack.Domain.Core;

namespace TransportTrack.Domain.Entities;

public class Ruta : BaseEntity
{
    [Required, MaxLength(100)] public string Origen { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Destino { get; set; } = string.Empty;
    public decimal DistanciaKm { get; set; }
    public DateTime FechaSalida { get; set; }
    public int VehiculoId { get; set; }
    public Vehiculo? Vehiculo { get; set; }
}
