using System.ComponentModel.DataAnnotations;

namespace TransportTrack.Application.Dtos.Route;

public class CreateRouteDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Origen { get; init; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 2)]
    public string Destino { get; init; } = string.Empty;
    [Range(typeof(decimal), "0.1", "1000000")]
    public decimal DistanciaKm { get; init; }
    [Required]
    public DateTime FechaSalida { get; init; }
    [Range(1, int.MaxValue)]
    public int VehiculoId { get; init; }
}
