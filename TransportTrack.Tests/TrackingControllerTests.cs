using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TransportTrack.Tests;

public class TrackingControllerTests : IClassFixture<TrackingApiFactory>
{
    private readonly HttpClient _client;
    private readonly TrackingApiFactory _factory;

    public TrackingControllerTests(TrackingApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var res = await _client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.Equal("healthy", body?.Status);
    }

    [Fact]
    public async Task FullFlow_EndToEnd()
    {
        var conductor = await _client.PostAsJsonAsync("/api/conductores", new
        {
            nombre = "Marcos Rodríguez",
            licencia = "CAT-04-88219",
            telefono = "809-555-0192",
        });
        conductor.EnsureSuccessStatusCode();
        var conductorBody = await conductor.Content.ReadFromJsonAsync<ConductorResponse>();
        Assert.NotNull(conductorBody);

        var pair = await _client.PostAsJsonAsync("/api/tracking/devices/pair", new
        {
            installationId = "e2e-device",
            platform = "web",
            name = "Test",
            conductorId = conductorBody!.Id,
        });
        Assert.Equal(HttpStatusCode.OK, pair.StatusCode);

        var sessionReq = new HttpRequestMessage(HttpMethod.Post, "/api/tracking/sessions");
        sessionReq.Headers.Add("X-Installation-Id", "e2e-device");
        sessionReq.Content = JsonContent.Create(new
        {
            conductorId = conductorBody.Id,
        });
        var session = await _client.SendAsync(sessionReq);
        session.EnsureSuccessStatusCode();
        var sessionBody = await session.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.NotNull(sessionBody);
        Assert.Equal("active", sessionBody.Status);

        using var pingReq = new HttpRequestMessage(HttpMethod.Post, "/api/tracking/pings");
        pingReq.Headers.Add("X-Installation-Id", "e2e-device");
        pingReq.Content = JsonContent.Create(new
        {
            clientPingId = Guid.NewGuid().ToString(),
            sessionId = sessionBody!.SessionId,
            latitude = 18.5041,
            longitude = -69.9153,
            accuracyMeters = 5,
            speedMetersPerSecond = 14.2,
            headingDegrees = 90,
            recordedAtUtc = DateTime.UtcNow.ToString("o"),
            source = "web",
        });
        var ping = await _client.SendAsync(pingReq);
        Assert.Equal(HttpStatusCode.OK, ping.StatusCode);

        var live = await _client.GetFromJsonAsync<LiveResponse[]>("/api/tracking/live");
        Assert.NotNull(live);
        Assert.Single(live);
        Assert.Equal(conductorBody.Id, live[0].ConductorId);
    }

    [Fact]
    public async Task Pair_UnknownConductor_Returns404()
    {
        var res = await _client.PostAsJsonAsync("/api/tracking/devices/pair", new
        {
            installationId = "dev-x",
            platform = "web",
            conductorId = 99999,
        });

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Ping_InvalidLatitude_Returns400()
    {
        var conductor = await _client.PostAsJsonAsync("/api/conductores", new
        {
            nombre = "Laura Méndez",
            licencia = "CAT-02-14533",
            telefono = "829-555-0117",
        });
        var conductorBody = await conductor.Content.ReadFromJsonAsync<ConductorResponse>();

        await _client.PostAsJsonAsync("/api/tracking/devices/pair", new
        {
            installationId = "dev-invalid",
            platform = "web",
            conductorId = conductorBody!.Id,
        });
        var session = await _client.PostAsJsonAsync("/api/tracking/sessions", new
        {
            conductorId = conductorBody.Id,
        });
        var sessionBody = await session.Content.ReadFromJsonAsync<SessionResponse>();

        using var pingReq = new HttpRequestMessage(HttpMethod.Post, "/api/tracking/pings");
        pingReq.Headers.Add("X-Installation-Id", "dev-invalid");
        pingReq.Content = JsonContent.Create(new
        {
            clientPingId = Guid.NewGuid().ToString(),
            sessionId = sessionBody!.SessionId,
            latitude = 95,
            longitude = -69.9,
            accuracyMeters = 5,
            recordedAtUtc = DateTime.UtcNow.ToString("o"),
            source = "web",
        });
        var res = await _client.SendAsync(pingReq);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    private sealed class HealthResponse
    {
        public string? Status { get; set; }
    }

    private sealed class ConductorResponse
    {
        public int Id { get; set; }
    }

    private sealed class SessionResponse
    {
        public int SessionId { get; set; }
        public string? Status { get; set; }
    }

    private sealed class LiveResponse
    {
        public int ConductorId { get; set; }
    }
}

public sealed class TrackingApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(
        Path.GetTempPath(),
        $"transporttrack-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_dbPath}");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}