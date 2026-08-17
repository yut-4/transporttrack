using TransportTrack.Application.Core;
using TransportTrack.Application.Interfaces;
using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Core;
using TransportTrack.Infrastructure.Exceptions;
using TransportTrack.Infrastructure.Interfaces;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Application.Services;

public class ConductorService : ServiceBase<Conductor>, IConductorService
{
    private readonly IConductorRepository _conductorRepository;

    public ConductorService(IConductorRepository conductorRepository)
        : base(conductorRepository as BaseRepository<Conductor> ?? throw new InvalidOperationException("Invalid repository type"))
    {
        _conductorRepository = conductorRepository;
    }

    public async Task<Conductor> CreateAsync(ConductorModel model)
    {
        var conductorExistente = await _conductorRepository.GetAllAsync();
        if (conductorExistente.Any(c => c.Licencia == model.Licencia))
        {
            throw new ConductorException("Ya existe un conductor con la licencia indicada.");
        }

        return await _conductorRepository.AddAsync(model);
    }

    public async Task UpdateAsync(int id, ConductorModel model)
    {
        var conductores = await _conductorRepository.GetAllAsync();
        var conductorExistente = conductores.FirstOrDefault(c => c.Licencia == model.Licencia && c.Id != id);
        if (conductorExistente is not null)
        {
            throw new ConductorException("Ya existe otro conductor con la licencia indicada.");
        }

        await _conductorRepository.UpdateAsync(id, model);
    }

    async Task<Conductor?> IConductorService.GetByIdAsync(int id)
    {
        return await _conductorRepository.GetByIdAsync(id);
    }

    async Task<List<Conductor>> IConductorService.GetAllAsync()
    {
        return await _conductorRepository.GetAllAsync();
    }

    async Task IConductorService.DeleteAsync(int id)
    {
        await _conductorRepository.DeleteAsync(id);
    }
}
