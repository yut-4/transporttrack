using System.ComponentModel.DataAnnotations;
using TransportTrack.Domain.Core;

namespace TransportTrack.Domain.Entities;

public enum TrackingSessionStatus
{
    Active = 0,
    Completed = 1,
    Cancelled = 2,
}

public class TrackingSession : BaseEntity
{
    [Required]
    public int ConductorId { get; set; }

    public int? VehiculoId { get; set; }

    public int? RutaId { get; set; }

    public int? TrackingDeviceId { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public DateTime? EndedAtUtc { get; set; }

    public TrackingSessionStatus Status { get; set; } = TrackingSessionStatus.Active;

    public Conductor Conductor { get; set; } = null!;

    public Vehiculo? Vehiculo { get; set; }

    public Ruta? Ruta { get; set; }

    public TrackingDevice? TrackingDevice { get; set; }

    public TrackingSession() : base()
    {
    }

    public TrackingSession(int conductorId, int? vehiculoId, int? rutaId, int? trackingDeviceId)
        : base()
    {
        ConductorId = conductorId;
        VehiculoId = vehiculoId;
        RutaId = rutaId;
        TrackingDeviceId = trackingDeviceId;
        StartedAtUtc = DateTime.UtcNow;
        Status = TrackingSessionStatus.Active;
    }
}