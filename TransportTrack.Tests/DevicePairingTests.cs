using TransportTrack.Infrastructure.Exceptions;

namespace TransportTrack.Tests;

public class DevicePairingTests
{
    [Fact]
    public async Task PairDevice_CreatesBinding()
    {
        using var ctx = new TrackingTestContext();

        var result = await ctx.PairAsync(ctx.Conductor.Id, "inst-abc");

        Assert.Equal(ctx.Conductor.Id, result.ConductorId);
        Assert.Equal("Marcos Rodríguez", result.ConductorNombre);
        Assert.True(result.DeviceId > 0);
    }

    [Fact]
    public async Task PairDevice_ReusesInstallationId()
    {
        using var ctx = new TrackingTestContext();

        var first = await ctx.PairAsync(ctx.Conductor.Id, "inst-abc");
        var second = await ctx.PairAsync(ctx.OtroConductor.Id, "inst-abc");

        Assert.Equal(first.DeviceId, second.DeviceId);
        Assert.Equal(ctx.OtroConductor.Id, second.ConductorId);
    }

    [Fact]
    public async Task PairDevice_UnknownConductor_Throws()
    {
        using var ctx = new TrackingTestContext();

        var ex = await Assert.ThrowsAsync<TrackingNotFoundException>(() =>
            ctx.Service.PairDeviceAsync(new TransportTrack.Infrastructure.Models.TrackingDevicePairRequest
            {
                InstallationId = "inst-x",
                ConductorId = 9999,
            }));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task PairDevice_EmptyInstallation_Throws()
    {
        using var ctx = new TrackingTestContext();

        var ex = await Assert.ThrowsAsync<TrackingValidationException>(() =>
            ctx.Service.PairDeviceAsync(new TransportTrack.Infrastructure.Models.TrackingDevicePairRequest
            {
                InstallationId = "",
                ConductorId = ctx.Conductor.Id,
            }));

        Assert.Equal(400, ex.StatusCode);
    }
}