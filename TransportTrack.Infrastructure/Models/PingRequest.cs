namespace TransportTrack.Infrastructure.Models;

public class PingRequest
{
    public string ClientPingId { get; set; } = string.Empty;

    public int SessionId { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double? AccuracyMeters { get; set; }

    public double? AltitudeMeters { get; set; }

    public double? SpeedMetersPerSecond { get; set; }

    public double? HeadingDegrees { get; set; }

    public DateTime RecordedAtUtc { get; set; }

    public string Source { get; set; } = "web";
}

public class BatchPingRequest
{
    public List<PingRequest> Pings { get; set; } = new();
}

public class PingResult
{
    public bool Accepted { get; set; }
}

public class BatchPingResult
{
    public int Accepted { get; set; }
}