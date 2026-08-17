using System.ComponentModel.DataAnnotations;
using TransportTrack.Domain.Core;

namespace TransportTrack.Domain.Entities;

public class LocationPing : BaseEntity
{
    [Required]
    [MaxLength(36)]
    public string ClientPingId { get; set; } = string.Empty;

    public int TrackingSessionId { get; set; }

    public int ConductorId { get; set; }

    public int? VehiculoId { get; set; }

    public int? RutaId { get; set; }

    public int? TrackingDeviceId { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double? AccuracyMeters { get; set; }

    public double? AltitudeMeters { get; set; }

    public double? SpeedMetersPerSecond { get; set; }

    public double? HeadingDegrees { get; set; }

    public DateTime RecordedAtUtc { get; set; }

    public DateTime ReceivedAtUtc { get; set; }

    [MaxLength(20)]
    public string Source { get; set; } = "web";

    public TrackingSession TrackingSession { get; set; } = null!;

    public Conductor Conductor { get; set; } = null!;

    public LocationPing() : base()
    {
    }

    public LocationPing(
        string clientPingId,
        int trackingSessionId,
        int conductorId,
        int? vehiculoId,
        int? rutaId,
        int? trackingDeviceId,
        double latitude,
        double longitude,
        double? accuracyMeters,
        double? altitudeMeters,
        double? speedMetersPerSecond,
        double? headingDegrees,
        DateTime recordedAtUtc,
        string source)
        : base()
    {
        ClientPingId = clientPingId;
        TrackingSessionId = trackingSessionId;
        ConductorId = conductorId;
        VehiculoId = vehiculoId;
        RutaId = rutaId;
        TrackingDeviceId = trackingDeviceId;
        Latitude = latitude;
        Longitude = longitude;
        AccuracyMeters = accuracyMeters;
        AltitudeMeters = altitudeMeters;
        SpeedMetersPerSecond = speedMetersPerSecond;
        HeadingDegrees = headingDegrees;
        RecordedAtUtc = recordedAtUtc;
        ReceivedAtUtc = DateTime.UtcNow;
        Source = source;
    }
}