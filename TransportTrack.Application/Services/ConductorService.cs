using TransportTrack.Application.Contract;
using TransportTrack.Application.Dtos.Conductor;
using TransportTrack.Infrastructure.Interfaces;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Application.Services;

public sealed class ConductorService(IConductorRepository repository) : IConductorService
{
    public async Task<IReadOnlyList<ConductorDto>> GetAllAsync() =>
        (await repository.GetAllAsync()).Select(Map).ToList();

    public async Task<ConductorDto?> GetByIdAsync(int id)
    {
        ServiceValidation.ValidateId(id);
        var entity = await repository.GetByIdAsync(id);
        return entity is null ? null : Map(entity);
    }

    public async Task<ConductorDto> CreateAsync(CreateConductorDto dto)
    {
        ServiceValidation.Validate(dto);
        return Map(await repository.AddAsync(ToModel(dto)));
    }

    public async Task UpdateAsync(int id, UpdateConductorDto dto)
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

    private static ConductorDto Map(TransportTrack.Domain.Entities.Conductor value) => new()
    {
        Id = value.Id, Nombre = value.Nombre, Licencia = value.Licencia, Telefono = value.Telefono
    };

    private static ConductorModel ToModel(CreateConductorDto value) => new()
    {
        Nombre = value.Nombre.Trim(), Licencia = value.Licencia.Trim(), Telefono = value.Telefono.Trim()
    };
}
