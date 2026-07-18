using TransportTrack.Application.Contract;
using TransportTrack.Application.Dtos.Route;
using TransportTrack.Infrastructure.Interfaces;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Application.Services;

public sealed class RouteService(IRutaRepository repository) : IRouteService
{
    public async Task<IReadOnlyList<RouteDto>> GetAllAsync() =>
        (await repository.GetAllAsync()).Select(Map).ToList();

    public async Task<RouteDto?> GetByIdAsync(int id)
    {
        ServiceValidation.ValidateId(id);
        var entity = await repository.GetByIdAsync(id);
        return entity is null ? null : Map(entity);
    }

    public async Task<RouteDto> CreateAsync(CreateRouteDto dto)
    {
        ServiceValidation.Validate(dto);
        return Map(await repository.AddAsync(ToModel(dto)));
    }

    public async Task UpdateAsync(int id, UpdateRouteDto dto)
    {
        ServiceValidation.ValidateId(id);
        ServiceValidation.Validate(dto);
        await repository.UpdateAsync(id, ToModel(dto));
    }

    public async Task DeleteAsync(int id)
    {
        ServiceValidation.ValidateId(id);
        await repository.DeleteAsync(id);
    }

    private static RouteDto Map(TransportTrack.Domain.Entities.Ruta value) => new()
    {
        Id = value.Id, Origen = value.Origen, Destino = value.Destino, DistanciaKm = value.DistanciaKm,
        FechaSalida = value.FechaSalida, VehiculoId = value.VehiculoId
    };

    private static RutaModel ToModel(CreateRouteDto value) => new()
    {
        Origen = value.Origen.Trim(), Destino = value.Destino.Trim(), DistanciaKm = value.DistanciaKm,
        FechaSalida = value.FechaSalida, VehiculoId = value.VehiculoId
    };
}
