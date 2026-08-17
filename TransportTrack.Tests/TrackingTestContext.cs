using Microsoft.EntityFrameworkCore;
using TransportTrack.Application.Interfaces;
using TransportTrack.Application.Services;
using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Context;
using TransportTrack.Infrastructure.Interfaces;
using TransportTrack.Infrastructure.Models;
using TransportTrack.Infrastructure.Repositories;

namespace TransportTrack.Tests;

public sealed class TrackingTestContext : IDisposable
{
    public TransportTrackContext Context { get; }
    public ITrackingService Service { get; }
    public Conductor Conductor { get; }
    public Conductor OtroConductor { get; }
    public Vehiculo Vehiculo { get; }

    public TrackingTestContext()
    {
        var options = new DbContextOptionsBuilder<TransportTrackContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        Context = new TransportTrackContext(options);

        Conductor = new Conductor("Marcos Rodríguez", "CAT-04-88219", "809-555-0192");
        OtroConductor = new Conductor("Laura Méndez", "CAT-02-14533", "829-555-0117");
        Context.Conductores.AddRange(Conductor, OtroConductor);
        Context.SaveChanges();

        Vehiculo = new Vehiculo("L428917", "Isuzu", "NPR", 2022, Conductor.Id) { Conductor = Conductor };
        Context.Vehiculos.Add(Vehiculo);
        Context.SaveChanges();

        var repository = new TrackingRepository(Context);
        var conductorRepository = new ConductorRepository(Context);
        Service = new TrackingService(repository, conductorRepository, Context);
    }

    public async Task<PairDeviceResponse> PairAsync(int conductorId, string installationId = "test-device")
    {
        return await Service.PairDeviceAsync(new TrackingDevicePairRequest
        {
            InstallationId = installationId,
            Platform = "web",
            Name = "Test browser",
            ConductorId = conductorId,
        });
    }

    public async Task<TrackingSessionDto> StartSessionAsync(
        string installationId = "test-device",
        int? vehiculoId = null)
    {
        return await Service.StartSessionAsync(new StartSessionRequest
        {
            ConductorId = Conductor.Id,
            VehiculoId = vehiculoId,
        }, installationId);
    }

    public static PingRequest ValidPing(int sessionId, string clientPingId, double lat = 18.5041, double lng = -69.9153)
    {
        return new PingRequest
        {
            ClientPingId = clientPingId,
            SessionId = sessionId,
            Latitude = lat,
            Longitude = lng,
            AccuracyMeters = 5,
            SpeedMetersPerSecond = 14.2,
            HeadingDegrees = 90,
            RecordedAtUtc = DateTime.UtcNow,
            Source = "web",
        };
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}