using TransportTrack.Infrastructure.Exceptions;

namespace TransportTrack.Tests;

public class TrackingSessionTests
{
    [Fact]
    public async Task StartSession_CreatesActiveSession()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);

        var session = await ctx.StartSessionAsync(vehiculoId: ctx.Vehiculo.Id);

        Assert.Equal(ctx.Conductor.Id, session.ConductorId);
        Assert.Equal("active", session.Status);
        Assert.Equal(ctx.Vehiculo.Id, session.VehiculoId);
        Assert.Equal("L428917", session.Placa);
        Assert.True(session.SessionId > 0);
    }

    [Fact]
    public async Task StartSession_DuplicateActive_Throws()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);
        await ctx.StartSessionAsync();

        var ex = await Assert.ThrowsAsync<TrackingValidationException>(() => ctx.StartSessionAsync());

        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("sesión activa", ex.Message);
    }

    [Fact]
    public async Task StartSession_WrongConductor_Throws()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.OtroConductor.Id);

        var ex = await Assert.ThrowsAsync<TrackingForbiddenException>(() =>
            ctx.Service.StartSessionAsync(new TransportTrack.Infrastructure.Models.StartSessionRequest
            {
                ConductorId = ctx.Conductor.Id,
            }, "test-device"));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task StartSession_VehicleNotOwned_Throws()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.OtroConductor.Id);

        var ex = await Assert.ThrowsAsync<TrackingValidationException>(() =>
            ctx.Service.StartSessionAsync(new TransportTrack.Infrastructure.Models.StartSessionRequest
            {
                ConductorId = ctx.OtroConductor.Id,
                VehiculoId = ctx.Vehiculo.Id,
            }, "test-device"));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task StopSession_CompletesActiveSession()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);
        var session = await ctx.StartSessionAsync();

        var stopped = await ctx.Service.StopSessionAsync(session.SessionId, ctx.Conductor.Id);

        Assert.Equal("completed", stopped.Status);
        Assert.True(stopped.StartedAtUtc != default);
    }

    [Fact]
    public async Task StopSession_UnknownSession_Throws()
    {
        using var ctx = new TrackingTestContext();
        await ctx.PairAsync(ctx.Conductor.Id);

        var ex = await Assert.ThrowsAsync<TrackingNotFoundException>(() =>
            ctx.Service.StopSessionAsync(9999, ctx.Conductor.Id));

        Assert.Equal(404, ex.StatusCode);
    }
}