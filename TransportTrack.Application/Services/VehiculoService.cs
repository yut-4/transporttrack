using TransportTrack.Application.Core;
using TransportTrack.Application.Interfaces;
using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Core;
using TransportTrack.Infrastructure.Exceptions;
using TransportTrack.Infrastructure.Interfaces;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Application.Services;

public class VehiculoService : ServiceBase<Vehiculo>, IVehiculoService
{
    private readonly IVehiculoRepository _vehiculoRepository;

    public VehiculoService(IVehiculoRepository vehiculoRepository)
        : base(vehiculoRepository as BaseRepository<Vehiculo> ?? throw new InvalidOperationException("Invalid repository type"))
    {
        _vehiculoRepository = vehiculoRepository;
    }

    public async Task<Vehiculo> CreateAsync(VehiculoModel model)
    {
        var vehiculos = await _vehiculoRepository.GetAllAsync();
        if (vehiculos.Any(v => v.Placa == model.Placa))
        {
            throw new VehiculoException("Ya existe un vehículo con la placa indicada.");
        }

        return await _vehiculoRepository.AddAsync(model);
    }

    public async Task UpdateAsync(int id, VehiculoModel model)
    {
        var vehiculos = await _vehiculoRepository.GetAllAsync();
        var vehiculoExistente = vehiculos.FirstOrDefault(v => v.Placa == model.Placa && v.Id != id);
        if (vehiculoExistente is not null)
        {
            throw new VehiculoException("Ya existe otro vehículo con la placa indicada.");
        }

        await _vehiculoRepository.UpdateAsync(id, model);
    }

    async Task<Vehiculo?> IVehiculoService.GetByIdAsync(int id)
    {
        return await _vehiculoRepository.GetByIdAsync(id);
    }

    async Task<List<Vehiculo>> IVehiculoService.GetAllAsync()
    {
        return await _vehiculoRepository.GetAllAsync();
    }

    async Task IVehiculoService.DeleteAsync(int id)
    {
        await _vehiculoRepository.DeleteAsync(id);
    }
}
