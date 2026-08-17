using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using TransportTrack.Api.Hubs;
using TransportTrack.Application.Interfaces;
using TransportTrack.Infrastructure.Exceptions;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Api.Controllers;

[ApiController]
[Route("api/tracking")]
public class TrackingController : ControllerBase
{
    private const string InstallationIdHeader = "X-Installation-Id";

    private readonly ITrackingService _trackingService;
    private readonly IHubContext<TrackingHub> _hubContext;

    public TrackingController(ITrackingService trackingService, IHubContext<TrackingHub> hubContext)
    {
        _trackingService = trackingService;
        _hubContext = hubContext;
    }

    [HttpPost("devices/pair")]
    [EnableRateLimiting("pair")]
    public async Task<ActionResult<PairDeviceResponse>> PairDevice([FromBody] TrackingDevicePairRequest request)
    {
        try
        {
            var result = await _trackingService.PairDeviceAsync(request);
            return Ok(result);
        }
        catch (TrackingException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPost("sessions")]
    [EnableRateLimiting("tracking-device")]
    public async Task<ActionResult<TrackingSessionDto>> StartSession([FromBody] StartSessionRequest request)
    {
        try
        {
            var installationId = GetInstallationId();
            var session = await _trackingService.StartSessionAsync(request, installationId);
            return Ok(session);
        }
        catch (TrackingException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPost("sessions/{sessionId:int}/stop")]
    [EnableRateLimiting("tracking-device")]
    public async Task<ActionResult<TrackingSessionDto>> StopSession(int sessionId, [FromQuery] int conductorId)
    {
        try
        {
            var session = await _trackingService.StopSessionAsync(sessionId, conductorId);
            return Ok(session);
        }
        catch (TrackingException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpGet("conductores/{conductorId:int}/active-session")]
    public async Task<ActionResult<TrackingSessionDto?>> GetActiveSession(int conductorId)
    {
        var session = await _trackingService.GetActiveSessionAsync(conductorId);
        return Ok(session);
    }

    [HttpPost("pings")]
    [EnableRateLimiting("tracking-device")]
    public async Task<ActionResult<PingResult>> AddPing([FromBody] PingRequest request)
    {
        try
        {
            var installationId = GetInstallationId();
            var result = await _trackingService.AddPingAsync(request, installationId);
            await BroadcastAsync(installationId);
            return Ok(result);
        }
        catch (TrackingException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPost("pings/batch")]
    [EnableRateLimiting("tracking-device")]
    public async Task<ActionResult<BatchPingResult>> AddPings([FromBody] BatchPingRequest request)
    {
        try
        {
            var installationId = GetInstallationId();
            var result = await _trackingService.AddPingsAsync(request, installationId);
            if (result.Accepted > 0)
            {
                await BroadcastAsync(installationId);
            }
            return Ok(result);
        }
        catch (TrackingException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpGet("live")]
    public async Task<ActionResult<List<DriverLocationDto>>> GetLive()
    {
        var locations = await _trackingService.GetLiveAsync();
        return Ok(locations);
    }

    [HttpGet("conductores/{conductorId:int}/latest")]
    public async Task<ActionResult<DriverLocationDto?>> GetLatest(int conductorId)
    {
        var location = await _trackingService.GetLatestAsync(conductorId);
        return Ok(location);
    }

    [HttpGet("conductores/{conductorId:int}/history")]
    public async Task<ActionResult<List<DriverLocationDto>>> GetHistory(int conductorId, [FromQuery] int limit = 200)
    {
        var history = await _trackingService.GetHistoryAsync(conductorId, limit);
        return Ok(history);
    }

    private string GetInstallationId()
    {
        return Request.Headers.TryGetValue(InstallationIdHeader, out var value)
            ? value.ToString()
            : string.Empty;
    }

    private async Task BroadcastAsync(string installationId)
    {
        if (string.IsNullOrWhiteSpace(installationId))
        {
            return;
        }

        var location = await _trackingService.GetLatestByInstallationAsync(installationId);
        if (location is not null)
        {
            await _hubContext.Clients.All.SendAsync("LocationUpdate", location);
        }
    }
}