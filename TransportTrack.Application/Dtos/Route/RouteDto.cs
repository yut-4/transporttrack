namespace TransportTrack.Application.Dtos.Route;

public sealed class RouteDto
{
    public int Id { get; init; }
    public string Origen { get; init; } = string.Empty;
    public string Destino { get; init; } = string.Empty;
    public decimal DistanciaKm { get; init; }
    public DateTime FechaSalida { get; init; }
    public int VehiculoId { get; init; }
}
