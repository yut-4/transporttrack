using System.ComponentModel.DataAnnotations;

namespace TransportTrack.Infrastructure.Models;

public class RutaModel
{
    [Required]
    [MaxLength(100)]
    public string Origen { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Destino { get; set; } = string.Empty;

    [Required]
    public int ConductorId { get; set; }

    [Required]
    public int VehiculoId { get; set; }

    public DateTime? FechaSalida { get; set; }

    public DateTime? FechaLlegada { get; set; }
}
