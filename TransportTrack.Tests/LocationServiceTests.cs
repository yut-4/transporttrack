using Microsoft.EntityFrameworkCore;

namespace TransportTrack.Tests;

public class LocationServiceTests
{
    [Fact]
    public async Task ValidPing_IsPersisted()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);
        var session = await ctx.StartSessionAsync(vehiculoId: ctx.Vehiculo.Id);

        var result = await ctx.Service.AddPingAsync(TrackingTestContext.ValidPing(session.SessionId, "p1"), "test-device");

        Assert.True(result.Accepted);
        var latest = await ctx.Service.GetLatestAsync(ctx.Conductor.Id);
        Assert.NotNull(latest);
        Assert.Equal(18.5041, latest!.Latitude);
        Assert.Equal(-69.9153, latest.Longitude);
        Assert.Equal("moving", latest.State);
    }

    [Fact]
    public async Task DuplicateClientPingId_IsIdempotent()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);
        var session = await ctx.StartSessionAsync();

        var first = await ctx.Service.AddPingAsync(TrackingTestContext.ValidPing(session.SessionId, "dup-1"), "test-device");
        var second = await ctx.Service.AddPingAsync(TrackingTestContext.ValidPing(session.SessionId, "dup-1"), "test-device");

        Assert.True(first.Accepted);
        Assert.True(second.Accepted);
        Assert.Equal(1, await ctx.Context.LocationPings.CountAsync());
    }

    [Fact]
    public async Task History_IsOrderedByRecordedAt()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);
        var session = await ctx.StartSessionAsync();

        var p1 = TrackingTestContext.ValidPing(session.SessionId, "h1", lat: 18.50);
        p1.RecordedAtUtc = DateTime.UtcNow.AddMinutes(-10);
        var p2 = TrackingTestContext.ValidPing(session.SessionId, "h2", lat: 18.51);
        p2.RecordedAtUtc = DateTime.UtcNow.AddMinutes(-5);
        await ctx.Service.AddPingAsync(p1, "test-device");
        await ctx.Service.AddPingAsync(p2, "test-device");

        var history = await ctx.Service.GetHistoryAsync(ctx.Conductor.Id, 200);

        Assert.Equal(2, history.Count);
        Assert.True(history[0].RecordedAtUtc >= history[1].RecordedAtUtc);
    }

    [Fact]
    public async Task BatchPings_AcceptsValidAndSkipsInvalid()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);
        var session = await ctx.StartSessionAsync();

        var valid1 = TrackingTestContext.ValidPing(session.SessionId, "b1");
        var valid2 = TrackingTestContext.ValidPing(session.SessionId, "b2");
        var invalid = TrackingTestContext.ValidPing(session.SessionId, "b3", lat: 200);
        invalid.SessionId = 9999;

        var result = await ctx.Service.AddPingsAsync(new TransportTrack.Infrastructure.Models.BatchPingRequest
        {
            Pings = [valid1, invalid, valid2],
        }, "test-device");

        Assert.Equal(2, result.Accepted);
        Assert.Equal(2, await ctx.Context.LocationPings.CountAsync());
    }

    [Fact]
    public async Task BatchPings_DuplicatesAreIdempotent()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);
        var session = await ctx.StartSessionAsync();

        var ping = TrackingTestContext.ValidPing(session.SessionId, "dup-batch");

        await ctx.Service.AddPingsAsync(new TransportTrack.Infrastructure.Models.BatchPingRequest
        {
            Pings = [ping],
        }, "test-device");
        var second = await ctx.Service.AddPingsAsync(new TransportTrack.Infrastructure.Models.BatchPingRequest
        {
            Pings = [ping],
        }, "test-device");

        Assert.Equal(1, second.Accepted);
        Assert.Equal(1, await ctx.Context.LocationPings.CountAsync());
    }

    [Fact]
    public async Task Live_ReturnsLatestPerConductor()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);
        var session = await ctx.StartSessionAsync(vehiculoId: ctx.Vehiculo.Id);

        var older = TrackingTestContext.ValidPing(session.SessionId, "live-1", lat: 18.50);
        older.RecordedAtUtc = DateTime.UtcNow.AddSeconds(-40);
        var newer = TrackingTestContext.ValidPing(session.SessionId, "live-2", lat: 18.51);
        newer.RecordedAtUtc = DateTime.UtcNow;
        await ctx.Service.AddPingAsync(older, "test-device");
        await ctx.Service.AddPingAsync(newer, "test-device");

        var live = await ctx.Service.GetLiveAsync();

        var item = Assert.Single(live);
        Assert.Equal("L428917", item.Placa);
        Assert.Equal(18.51, item.Latitude);
    }
}