using System.ComponentModel.DataAnnotations;
using TransportTrack.Domain.Core;

namespace TransportTrack.Domain.Entities;

public class TrackingDevice : BaseEntity
{
    [Required]
    public int ConductorId { get; set; }

    [Required]
    [MaxLength(64)]
    public string InstallationId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(20)]
    public string Platform { get; set; } = "web";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? LastSeenAtUtc { get; set; }

    [MaxLength(128)]
    public string? TokenHash { get; set; }

    public Conductor Conductor { get; set; } = null!;

    public TrackingDevice() : base()
    {
    }

    public TrackingDevice(int conductorId, string installationId, string platform, string? name)
        : base()
    {
        ConductorId = conductorId;
        InstallationId = installationId;
        Platform = platform;
        Name = name;
        CreatedAtUtc = DateTime.UtcNow;
        IsActive = true;
    }
}