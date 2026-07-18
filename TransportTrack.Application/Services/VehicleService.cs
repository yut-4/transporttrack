using TransportTrack.Application.Contract;
using TransportTrack.Application.Dtos.Vehicle;
using TransportTrack.Infrastructure.Interfaces;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Application.Services;

public sealed class VehicleService(IVehiculoRepository repository) : IVehicleService
{
    public async Task<IReadOnlyList<VehicleDto>> GetAllAsync() =>
        (await repository.GetAllAsync()).Select(Map).ToList();

    public async Task<VehicleDto?> GetByIdAsync(int id)
    {
        ServiceValidation.ValidateId(id);
        var entity = await repository.GetByIdAsync(id);
        return entity is null ? null : Map(entity);
    }

    public async Task<VehicleDto> CreateAsync(CreateVehicleDto dto)
    {
        ServiceValidation.Validate(dto);
        return Map(await repository.AddAsync(ToModel(dto)));
    }

    public async Task UpdateAsync(int id, UpdateVehicleDto dto)
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

    private static VehicleDto Map(TransportTrack.Domain.Entities.Vehiculo value) => new()
    {
        Id = value.Id, Placa = value.Placa, Marca = value.Marca, Modelo = value.Modelo,
        Anio = value.Anio, ConductorId = value.ConductorId, NombreConductor = value.Conductor?.Nombre
    };

    private static VehiculoModel ToModel(CreateVehicleDto value) => new()
    {
        Placa = value.Placa.Trim().ToUpperInvariant(), Marca = value.Marca.Trim(),
        Modelo = value.Modelo.Trim(), Anio = value.Anio, ConductorId = value.ConductorId
    };
}
