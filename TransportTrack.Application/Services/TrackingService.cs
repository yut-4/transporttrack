using Microsoft.EntityFrameworkCore;
using TransportTrack.Application.Interfaces;
using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Context;
using TransportTrack.Infrastructure.Exceptions;
using TransportTrack.Infrastructure.Interfaces;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Application.Services;

public class TrackingService : ITrackingService
{
    private const int LiveWindowSeconds = 300;
    private const int LiveThresholdSeconds = 90;
    private const double MovingSpeedThreshold = 2.0;

    private readonly ITrackingRepository _repository;
    private readonly IConductorRepository _conductorRepository;
    private readonly TransportTrackContext _context;

    public TrackingService(
        ITrackingRepository repository,
        IConductorRepository conductorRepository,
        TransportTrackContext context)
    {
        _repository = repository;
        _conductorRepository = conductorRepository;
        _context = context;
    }

    public async Task<PairDeviceResponse> PairDeviceAsync(TrackingDevicePairRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.InstallationId) || request.InstallationId.Length > 64)
        {
            throw new TrackingValidationException("El identificador de instalación es obligatorio.");
        }

        var conductor = await _conductorRepository.GetByIdAsync(request.ConductorId)
            ?? throw new TrackingNotFoundException("El conductor indicado no existe.");

        var device = await _repository.GetDeviceByInstallationIdAsync(request.InstallationId);
        if (device is null)
        {
            device = new TrackingDevice(
                conductor.Id,
                request.InstallationId.Trim(),
                string.IsNullOrWhiteSpace(request.Platform) ? "web" : request.Platform,
                string.IsNullOrWhiteSpace(request.Name) ? null : request.Name);
            await _repository.AddDeviceAsync(device);
        }
        else
        {
            device.ConductorId = conductor.Id;
            device.Name = string.IsNullOrWhiteSpace(request.Name) ? device.Name : request.Name;
            device.Platform = string.IsNullOrWhiteSpace(request.Platform) ? device.Platform : request.Platform;
            device.IsActive = true;
            device.LastSeenAtUtc = DateTime.UtcNow;
            await _repository.UpdateDeviceAsync(device);
        }

        return new PairDeviceResponse
        {
            DeviceId = device.Id,
            ConductorId = conductor.Id,
            ConductorNombre = conductor.Nombre,
        };
    }

    public async Task<TrackingSessionDto> StartSessionAsync(StartSessionRequest request, string installationId)
    {
        var device = await ResolveDeviceAsync(installationId);

        if (request.ConductorId != device.ConductorId)
        {
            throw new TrackingForbiddenException("La sesión debe corresponder al conductor vinculado al dispositivo.");
        }

        if (request.VehiculoId.HasValue)
        {
            var vehiculo = await _context.Vehiculos
                .FirstOrDefaultAsync(v => v.Id == request.VehiculoId.Value && !v.IsDeleted)
                ?? throw new TrackingValidationException("El vehículo indicado no existe.");
            if (vehiculo.ConductorId != device.ConductorId)
            {
                throw new TrackingValidationException("El vehículo no corresponde al conductor.");
            }
        }

        var existing = await _repository.GetActiveSessionAsync(device.ConductorId);
        if (existing is not null)
        {
            throw new TrackingValidationException("El conductor ya tiene una sesión activa.");
        }

        var session = new TrackingSession(
            device.ConductorId,
            request.VehiculoId,
            request.RutaId,
            device.Id);
        await _repository.AddSessionAsync(session);

        device.LastSeenAtUtc = DateTime.UtcNow;
        await _repository.UpdateDeviceAsync(device);

        return await GetSessionDtoAsync(session.Id) ?? throw new TrackingException("No se pudo crear la sesión.");
    }

    public async Task<TrackingSessionDto> StopSessionAsync(int sessionId, int conductorId)
    {
        var session = await _repository.GetSessionForConductorAsync(sessionId, conductorId)
            ?? throw new TrackingNotFoundException("La sesión indicada no existe.");

        if (session.Status == TrackingSessionStatus.Active)
        {
            session.Status = TrackingSessionStatus.Completed;
            session.EndedAtUtc = DateTime.UtcNow;
            await _repository.UpdateSessionAsync(session);
        }

        return ToSessionDto(session);
    }

    public async Task<TrackingSessionDto?> GetActiveSessionAsync(int conductorId)
    {
        var session = await _repository.GetActiveSessionAsync(conductorId);
        return session is null ? null : ToSessionDto(session);
    }

    public async Task<PingResult> AddPingAsync(PingRequest request, string installationId)
    {
        var device = await ResolveDeviceAsync(installationId);
        var session = await ValidateSessionForDeviceAsync(request, device);

        if (await _repository.ClientPingExistsAsync(request.ClientPingId))
        {
            return new PingResult { Accepted = true };
        }

        var ping = BuildPing(request, session, device);
        await _repository.AddPingAsync(ping);

        await TouchDeviceAsync(device);
        return new PingResult { Accepted = true };
    }

    public async Task<BatchPingResult> AddPingsAsync(BatchPingRequest request, string installationId)
    {
        var device = await ResolveDeviceAsync(installationId);
        var accepted = 0;
        var newPings = new List<LocationPing>();

        foreach (var ping in request.Pings ?? new List<PingRequest>())
        {
            if (!TryValidatePing(ping, out var validationError))
            {
                continue;
            }

            if (await _repository.ClientPingExistsAsync(ping.ClientPingId))
            {
                accepted++;
                continue;
            }

            TrackingSession? session;
            try
            {
                session = await _repository.GetSessionByIdAsync(ping.SessionId);
            }
            catch
            {
                session = null;
            }

            if (session is null
                || session.ConductorId != device.ConductorId
                || session.Status != TrackingSessionStatus.Active)
            {
                continue;
            }

            newPings.Add(BuildPing(ping, session, device));
            accepted++;
        }

        if (newPings.Count > 0)
        {
            await _repository.AddPingsAsync(newPings);
        }

        await TouchDeviceAsync(device);
        return new BatchPingResult { Accepted = accepted };
    }

    public async Task<List<DriverLocationDto>> GetLiveAsync()
    {
        var pings = await _repository.GetRecentPingsAsync(LiveWindowSeconds, 500);
        if (pings.Count == 0)
        {
            return new List<DriverLocationDto>();
        }

        var conductorIds = pings.Select(p => p.ConductorId).Distinct().ToList();
        var conductores = await _context.Conductores
            .Where(c => conductorIds.Contains(c.Id) && !c.IsDeleted)
            .ToDictionaryAsync(c => c.Id);
        var vehiculoIds = pings.Select(p => p.VehiculoId ?? 0).Where(id => id > 0).Distinct().ToList();
        var vehiculos = vehiculoIds.Count > 0
            ? await _context.Vehiculos.Where(v => vehiculoIds.Contains(v.Id)).ToDictionaryAsync(v => v.Id)
            : new Dictionary<int, Vehiculo>();
        var rutaIds = pings.Select(p => p.RutaId ?? 0).Where(id => id > 0).Distinct().ToList();
        var rutas = rutaIds.Count > 0
            ? await _context.Rutas.Where(r => rutaIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id)
            : new Dictionary<int, Ruta>();

        var result = new List<DriverLocationDto>();
        foreach (var ping in pings)
        {
            conductores.TryGetValue(ping.ConductorId, out var conductor);
            vehiculos.TryGetValue(ping.VehiculoId ?? 0, out var vehiculo);
            rutas.TryGetValue(ping.RutaId ?? 0, out var ruta);
            result.Add(ToDriverLocation(ping, conductor, vehiculo, ruta));
        }

        return result.OrderByDescending(x => x.RecordedAtUtc).ToList();
    }

    public async Task<DriverLocationDto?> GetLatestAsync(int conductorId)
    {
        var ping = await _repository.GetLatestAsync(conductorId);
        if (ping is null)
        {
            return null;
        }

        var conductor = await _context.Conductores.FirstOrDefaultAsync(c => c.Id == conductorId);
        var vehiculo = ping.VehiculoId.HasValue
            ? await _context.Vehiculos.FirstOrDefaultAsync(v => v.Id == ping.VehiculoId.Value)
            : null;
        var ruta = ping.RutaId.HasValue
            ? await _context.Rutas.FirstOrDefaultAsync(r => r.Id == ping.RutaId.Value)
            : null;

        return ToDriverLocation(ping, conductor, vehiculo, ruta);
    }

    public async Task<DriverLocationDto?> GetLatestByInstallationAsync(string installationId)
    {
        var device = await _repository.GetDeviceByInstallationIdAsync(installationId?.Trim() ?? string.Empty);
        if (device is null || !device.IsActive)
        {
            return null;
        }

        return await GetLatestAsync(device.ConductorId);
    }

    public async Task<List<DriverLocationDto>> GetHistoryAsync(int conductorId, int limit)
    {
        if (limit < 1 || limit > 1000)
        {
            limit = 200;
        }

        var pings = await _repository.GetHistoryAsync(conductorId, limit);
        var result = new List<DriverLocationDto>();
        foreach (var ping in pings)
        {
            result.Add(new DriverLocationDto
            {
                ConductorId = ping.ConductorId,
                ConductorNombre = string.Empty,
                VehiculoId = ping.VehiculoId,
                RutaId = ping.RutaId,
                Latitude = ping.Latitude,
                Longitude = ping.Longitude,
                AccuracyMeters = ping.AccuracyMeters,
                SpeedMetersPerSecond = ping.SpeedMetersPerSecond,
                HeadingDegrees = ping.HeadingDegrees,
                RecordedAtUtc = ping.RecordedAtUtc,
                ReceivedAtUtc = ping.ReceivedAtUtc,
                State = ComputeState(ping.RecordedAtUtc, ping.SpeedMetersPerSecond),
            });
        }

        return result.OrderByDescending(x => x.RecordedAtUtc).ToList();
    }

    private async Task<TrackingDevice> ResolveDeviceAsync(string installationId)
    {
        if (string.IsNullOrWhiteSpace(installationId))
        {
            throw new TrackingUnauthorizedException("Dispositivo no identificado.");
        }

        var device = await _repository.GetDeviceByInstallationIdAsync(installationId.Trim());
        if (device is null || !device.IsActive)
        {
            throw new TrackingUnauthorizedException("Dispositivo no vinculado. Ejecute el emparejamiento primero.");
        }

        return device;
    }

    private async Task<TrackingSession> ValidateSessionForDeviceAsync(PingRequest request, TrackingDevice device)
    {
        if (!TryValidatePing(request, out var error))
        {
            throw new TrackingValidationException(error);
        }

        var session = await _repository.GetSessionByIdAsync(request.SessionId)
            ?? throw new TrackingNotFoundException("La sesión indicada no existe.");

        if (session.ConductorId != device.ConductorId)
        {
            throw new TrackingForbiddenException("La sesión no corresponde al dispositivo.");
        }

        if (session.Status != TrackingSessionStatus.Active)
        {
            throw new TrackingValidationException("La sesión no está activa.");
        }

        return session;
    }

    private static bool TryValidatePing(PingRequest request, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(request.ClientPingId) || request.ClientPingId.Length > 36)
        {
            error = "El identificador de ping es obligatorio.";
            return false;
        }

        if (request.Latitude is < -90 or > 90)
        {
            error = "La latitud está fuera de rango.";
            return false;
        }

        if (request.Longitude is < -180 or > 180)
        {
            error = "La longitud está fuera de rango.";
            return false;
        }

        if (request.AccuracyMeters is < 0)
        {
            error = "La precisión no puede ser negativa.";
            return false;
        }

        if (request.SpeedMetersPerSecond is < 0)
        {
            error = "La velocidad no puede ser negativa.";
            return false;
        }

        if (request.HeadingDegrees is < 0 or > 360)
        {
            error = "El rumbo debe estar entre 0 y 360 grados.";
            return false;
        }

        if (request.RecordedAtUtc == default)
        {
            request.RecordedAtUtc = DateTime.UtcNow;
        }

        return true;
    }

    private static LocationPing BuildPing(PingRequest request, TrackingSession session, TrackingDevice device)
    {
        var recordedAt = request.RecordedAtUtc > DateTime.UtcNow.AddMinutes(5)
            ? DateTime.UtcNow
            : request.RecordedAtUtc;

        return new LocationPing(
            request.ClientPingId,
            session.Id,
            device.ConductorId,
            session.VehiculoId,
            session.RutaId,
            device.Id,
            request.Latitude,
            request.Longitude,
            request.AccuracyMeters,
            request.AltitudeMeters,
            request.SpeedMetersPerSecond,
            request.HeadingDegrees,
            recordedAt,
            string.IsNullOrWhiteSpace(request.Source) ? "web" : request.Source);
    }

    private async Task TouchDeviceAsync(TrackingDevice device)
    {
        device.LastSeenAtUtc = DateTime.UtcNow;
        await _repository.UpdateDeviceAsync(device);
    }

    private async Task<TrackingSessionDto?> GetSessionDtoAsync(int sessionId)
    {
        var session = await _repository.GetSessionByIdAsync(sessionId);
        return session is null ? null : ToSessionDto(session);
    }

    private static TrackingSessionDto ToSessionDto(TrackingSession session)
    {
        return new TrackingSessionDto
        {
            SessionId = session.Id,
            ConductorId = session.ConductorId,
            ConductorNombre = session.Conductor?.Nombre ?? string.Empty,
            VehiculoId = session.VehiculoId,
            Placa = session.Vehiculo?.Placa,
            RutaId = session.RutaId,
            Origen = session.Ruta?.Origen,
            Destino = session.Ruta?.Destino,
            Status = session.Status.ToString().ToLowerInvariant(),
            StartedAtUtc = session.StartedAtUtc,
            DeviceName = session.TrackingDevice?.Name,
        };
    }

    private static DriverLocationDto ToDriverLocation(
        LocationPing ping,
        Conductor? conductor,
        Vehiculo? vehiculo,
        Ruta? ruta)
    {
        return new DriverLocationDto
        {
            ConductorId = ping.ConductorId,
            ConductorNombre = conductor?.Nombre ?? string.Empty,
            VehiculoId = ping.VehiculoId,
            Placa = vehiculo?.Placa,
            RutaId = ping.RutaId,
            Origen = ruta?.Origen,
            Destino = ruta?.Destino,
            Latitude = ping.Latitude,
            Longitude = ping.Longitude,
            AccuracyMeters = ping.AccuracyMeters,
            SpeedMetersPerSecond = ping.SpeedMetersPerSecond,
            HeadingDegrees = ping.HeadingDegrees,
            RecordedAtUtc = ping.RecordedAtUtc,
            ReceivedAtUtc = ping.ReceivedAtUtc,
            State = ComputeState(ping.RecordedAtUtc, ping.SpeedMetersPerSecond),
        };
    }

    private static string ComputeState(DateTime recordedAtUtc, double? speed)
    {
        var age = DateTime.UtcNow - recordedAtUtc;
        if (age.TotalSeconds <= LiveThresholdSeconds)
        {
            return speed.HasValue && speed.Value >= MovingSpeedThreshold ? "moving" : "stopped";
        }

        if (age.TotalSeconds <= LiveWindowSeconds)
        {
            return "stale";
        }

        return "offline";
    }
}