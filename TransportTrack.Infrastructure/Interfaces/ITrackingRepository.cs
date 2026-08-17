using TransportTrack.Domain.Entities;

namespace TransportTrack.Infrastructure.Interfaces;

public interface ITrackingRepository
{
    Task<TrackingDevice?> GetDeviceByInstallationIdAsync(string installationId);

    Task<TrackingDevice?> GetActiveDeviceByConductorAsync(int conductorId);

    Task AddDeviceAsync(TrackingDevice device);

    Task UpdateDeviceAsync(TrackingDevice device);

    Task<TrackingSession?> GetActiveSessionAsync(int conductorId);

    Task<TrackingSession?> GetSessionByIdAsync(int sessionId);

    Task<TrackingSession?> GetSessionForConductorAsync(int sessionId, int conductorId);

    Task AddSessionAsync(TrackingSession session);

    Task UpdateSessionAsync(TrackingSession session);

    Task<bool> ClientPingExistsAsync(string clientPingId);

    Task AddPingAsync(LocationPing ping);

    Task AddPingsAsync(IEnumerable<LocationPing> pings);

    Task<LocationPing?> GetLatestAsync(int conductorId);

    Task<List<LocationPing>> GetHistoryAsync(int conductorId, int limit);

    Task<List<LocationPing>> GetRecentPingsAsync(int staleAfterSeconds, int limit);
}