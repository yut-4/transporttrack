namespace TransportTrack.Infrastructure.Models;

public class TrackingSessionDto
{
    public int SessionId { get; set; }

    public int ConductorId { get; set; }

    public string ConductorNombre { get; set; } = string.Empty;

    public int? VehiculoId { get; set; }

    public string? Placa { get; set; }

    public int? RutaId { get; set; }

    public string? Origen { get; set; }

    public string? Destino { get; set; }

    public string Status { get; set; } = "active";

    public DateTime StartedAtUtc { get; set; }

    public string? DeviceName { get; set; }
}