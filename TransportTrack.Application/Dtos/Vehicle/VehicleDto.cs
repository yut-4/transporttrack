namespace TransportTrack.Application.Dtos.Vehicle;

public sealed class VehicleDto
{
    public int Id { get; init; }
    public string Placa { get; init; } = string.Empty;
    public string Marca { get; init; } = string.Empty;
    public string Modelo { get; init; } = string.Empty;
    public int Anio { get; init; }
    public int ConductorId { get; init; }
    public string? NombreConductor { get; init; }
}
