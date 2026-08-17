using Microsoft.AspNetCore.SignalR;
using TransportTrack.Application.Interfaces;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Api.Hubs;

public class TrackingHub : Hub
{
    private readonly ITrackingService _trackingService;

    public TrackingHub(ITrackingService trackingService)
    {
        _trackingService = trackingService;
    }

    public async Task<List<DriverLocationDto>> GetLive()
    {
        return await _trackingService.GetLiveAsync();
    }
}