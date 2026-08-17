namespace TransportTrack.Infrastructure.Models;

public class DriverLocationDto
{
    public int ConductorId { get; set; }

    public string ConductorNombre { get; set; } = string.Empty;

    public int? VehiculoId { get; set; }

    public string? Placa { get; set; }

    public int? RutaId { get; set; }

    public string? Origen { get; set; }

    public string? Destino { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double? AccuracyMeters { get; set; }

    public double? SpeedMetersPerSecond { get; set; }

    public double? HeadingDegrees { get; set; }

    public DateTime RecordedAtUtc { get; set; }

    public DateTime ReceivedAtUtc { get; set; }

    public string State { get; set; } = "offline";
}