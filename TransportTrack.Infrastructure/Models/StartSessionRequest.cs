namespace TransportTrack.Infrastructure.Models;

public class StartSessionRequest
{
    public int ConductorId { get; set; }

    public int? VehiculoId { get; set; }

    public int? RutaId { get; set; }
}