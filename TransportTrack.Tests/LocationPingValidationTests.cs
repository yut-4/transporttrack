using TransportTrack.Infrastructure.Exceptions;

namespace TransportTrack.Tests;

public class LocationPingValidationTests
{
    private async Task<(TrackingTestContext ctx, int sessionId)> SetupWithSessionAsync()
    {
        var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);
        var session = await ctx.StartSessionAsync(vehiculoId: ctx.Vehiculo.Id);
        return (ctx, session.SessionId);
    }

    [Fact]
    public async Task Latitude_OutOfRange_Throws()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);
        var session = await ctx.StartSessionAsync();
        var ping = TrackingTestContext.ValidPing(session.SessionId, "p1", lat: 95);

        var ex = await Assert.ThrowsAsync<TrackingValidationException>(() =>
            ctx.Service.AddPingAsync(ping, "test-device"));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task Longitude_OutOfRange_Throws()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);
        var session = await ctx.StartSessionAsync();
        var ping = TrackingTestContext.ValidPing(session.SessionId, "p1", lng: -200);

        var ex = await Assert.ThrowsAsync<TrackingValidationException>(() =>
            ctx.Service.AddPingAsync(ping, "test-device"));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task NegativeAccuracy_Throws()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);
        var session = await ctx.StartSessionAsync();
        var ping = TrackingTestContext.ValidPing(session.SessionId, "p1");
        ping.AccuracyMeters = -1;

        var ex = await Assert.ThrowsAsync<TrackingValidationException>(() =>
            ctx.Service.AddPingAsync(ping, "test-device"));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task Heading_OutOfRange_Throws()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);
        var session = await ctx.StartSessionAsync();
        var ping = TrackingTestContext.ValidPing(session.SessionId, "p1");
        ping.HeadingDegrees = 400;

        var ex = await Assert.ThrowsAsync<TrackingValidationException>(() =>
            ctx.Service.AddPingAsync(ping, "test-device"));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task UnknownDevice_Throws()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);
        var session = await ctx.StartSessionAsync();
        var ping = TrackingTestContext.ValidPing(session.SessionId, "p1");

        var ex = await Assert.ThrowsAsync<TrackingUnauthorizedException>(() =>
            ctx.Service.AddPingAsync(ping, "device-no-existe"));

        Assert.Equal(401, ex.StatusCode);
    }

    [Fact]
    public async Task UnknownSession_Throws()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);
        var ping = TrackingTestContext.ValidPing(9999, "p1");

        var ex = await Assert.ThrowsAsync<TrackingNotFoundException>(() =>
            ctx.Service.AddPingAsync(ping, "test-device"));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task SessionOfOtherConductor_Throws()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.OtroConductor.Id);
        var sessionOtro = await ctx.Service.StartSessionAsync(
            new TransportTrack.Infrastructure.Models.StartSessionRequest
            {
                ConductorId = ctx.OtroConductor.Id,
            }, "test-device");

        await ctx.PairAsync(ctx.Conductor.Id, "device-2");
        var ping = TrackingTestContext.ValidPing(sessionOtro.SessionId, "p1");

        var ex = await Assert.ThrowsAsync<TrackingForbiddenException>(() =>
            ctx.Service.AddPingAsync(ping, "device-2"));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task InactiveSession_RejectsPing()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);
        var session = await ctx.StartSessionAsync();
        await ctx.Service.StopSessionAsync(session.SessionId, ctx.Conductor.Id);

        var ping = TrackingTestContext.ValidPing(session.SessionId, "p1");

        var ex = await Assert.ThrowsAsync<TrackingValidationException>(() =>
            ctx.Service.AddPingAsync(ping, "test-device"));

        Assert.Contains("no está activa", ex.Message);
    }
}