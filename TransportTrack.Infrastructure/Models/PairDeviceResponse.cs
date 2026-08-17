namespace TransportTrack.Infrastructure.Models;

public class PairDeviceResponse
{
    public int DeviceId { get; set; }

    public int ConductorId { get; set; }

    public string ConductorNombre { get; set; } = string.Empty;
}