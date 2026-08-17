using Microsoft.EntityFrameworkCore;
using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Context;
using TransportTrack.Infrastructure.Interfaces;

namespace TransportTrack.Infrastructure.Repositories;

public class TrackingRepository : ITrackingRepository
{
    private readonly TransportTrackContext _context;

    public TrackingRepository(TransportTrackContext context)
    {
        _context = context;
    }

    public async Task<TrackingDevice?> GetDeviceByInstallationIdAsync(string installationId)
    {
        return await _context.TrackingDevices
            .FirstOrDefaultAsync(d => d.InstallationId == installationId);
    }

    public async Task<TrackingDevice?> GetActiveDeviceByConductorAsync(int conductorId)
    {
        return await _context.TrackingDevices
            .Where(d => d.ConductorId == conductorId && d.IsActive)
            .OrderByDescending(d => d.LastSeenAtUtc)
            .FirstOrDefaultAsync();
    }

    public async Task AddDeviceAsync(TrackingDevice device)
    {
        _context.TrackingDevices.Add(device);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateDeviceAsync(TrackingDevice device)
    {
        _context.TrackingDevices.Update(device);
        await _context.SaveChangesAsync();
    }

    public async Task<TrackingSession?> GetActiveSessionAsync(int conductorId)
    {
        return await _context.TrackingSessions
            .Include(s => s.Conductor)
            .Include(s => s.Vehiculo)
            .Include(s => s.Ruta)
            .Include(s => s.TrackingDevice)
            .Where(s => s.ConductorId == conductorId && s.Status == TrackingSessionStatus.Active)
            .OrderByDescending(s => s.StartedAtUtc)
            .FirstOrDefaultAsync();
    }

    public async Task<TrackingSession?> GetSessionByIdAsync(int sessionId)
    {
        return await _context.TrackingSessions
            .Include(s => s.Conductor)
            .Include(s => s.Vehiculo)
            .Include(s => s.Ruta)
            .Include(s => s.TrackingDevice)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<TrackingSession?> GetSessionForConductorAsync(int sessionId, int conductorId)
    {
        return await _context.TrackingSessions
            .Include(s => s.Conductor)
            .Include(s => s.Vehiculo)
            .Include(s => s.Ruta)
            .Include(s => s.TrackingDevice)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.ConductorId == conductorId);
    }

    public async Task AddSessionAsync(TrackingSession session)
    {
        _context.TrackingSessions.Add(session);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSessionAsync(TrackingSession session)
    {
        _context.TrackingSessions.Update(session);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ClientPingExistsAsync(string clientPingId)
    {
        return await _context.LocationPings
            .AnyAsync(p => p.ClientPingId == clientPingId);
    }

    public async Task AddPingAsync(LocationPing ping)
    {
        _context.LocationPings.Add(ping);
        await _context.SaveChangesAsync();
    }

    public async Task AddPingsAsync(IEnumerable<LocationPing> pings)
    {
        var list = pings.ToList();
        if (list.Count == 0)
            return;
        _context.LocationPings.AddRange(list);
        await _context.SaveChangesAsync();
    }

    public async Task<LocationPing?> GetLatestAsync(int conductorId)
    {
        return await _context.LocationPings
            .AsNoTracking()
            .Where(p => p.ConductorId == conductorId)
            .OrderByDescending(p => p.RecordedAtUtc)
            .FirstOrDefaultAsync();
    }

    public async Task<List<LocationPing>> GetHistoryAsync(int conductorId, int limit)
    {
        return await _context.LocationPings
            .AsNoTracking()
            .Where(p => p.ConductorId == conductorId)
            .OrderByDescending(p => p.RecordedAtUtc)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<LocationPing>> GetRecentPingsAsync(int staleAfterSeconds, int limit)
    {
        var cutoff = DateTime.UtcNow.AddSeconds(-staleAfterSeconds);
        var cutoff5Min = DateTime.UtcNow.AddMinutes(-5);

        var latest = await _context.LocationPings
            .AsNoTracking()
            .Where(p => p.RecordedAtUtc >= cutoff5Min)
            .OrderBy(p => p.ConductorId)
            .ThenByDescending(p => p.RecordedAtUtc)
            .ToListAsync();

        return latest
            .GroupBy(p => p.ConductorId)
            .Select(g => g.First())
            .ToList();
    }
}