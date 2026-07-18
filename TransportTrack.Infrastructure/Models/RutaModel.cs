using System.ComponentModel.DataAnnotations;

namespace TransportTrack.Infrastructure.Models;

public class RutaModel
{
    [Required, StringLength(100)] public string Origen { get; set; } = string.Empty;
    [Required, StringLength(100)] public string Destino { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.1", "1000000")] public decimal DistanciaKm { get; set; }
    public DateTime FechaSalida { get; set; }
    [Range(1, int.MaxValue)] public int VehiculoId { get; set; }
}
