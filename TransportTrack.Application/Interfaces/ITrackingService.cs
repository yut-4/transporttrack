using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Application.Interfaces;

public interface ITrackingService
{
    Task<PairDeviceResponse> PairDeviceAsync(TrackingDevicePairRequest request);

    Task<TrackingSessionDto> StartSessionAsync(StartSessionRequest request, string installationId);

    Task<TrackingSessionDto> StopSessionAsync(int sessionId, int conductorId);

    Task<TrackingSessionDto?> GetActiveSessionAsync(int conductorId);

    Task<PingResult> AddPingAsync(PingRequest request, string installationId);

    Task<BatchPingResult> AddPingsAsync(BatchPingRequest request, string installationId);

    Task<List<DriverLocationDto>> GetLiveAsync();

    Task<DriverLocationDto?> GetLatestAsync(int conductorId);

    Task<DriverLocationDto?> GetLatestByInstallationAsync(string installationId);

    Task<List<DriverLocationDto>> GetHistoryAsync(int conductorId, int limit);
}