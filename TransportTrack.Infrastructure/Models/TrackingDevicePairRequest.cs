namespace TransportTrack.Infrastructure.Models;

public class TrackingDevicePairRequest
{
    public string InstallationId { get; set; } = string.Empty;

    public string Platform { get; set; } = "web";

    public string? Name { get; set; }

    public int ConductorId { get; set; }
}