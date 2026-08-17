using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Application.Interfaces;

public interface IVehiculoService
{
    Task<List<Vehiculo>> GetAllAsync();
    Task<Vehiculo?> GetByIdAsync(int id);
    Task<Vehiculo> CreateAsync(VehiculoModel model);
    Task UpdateAsync(int id, VehiculoModel model);
    Task DeleteAsync(int id);
}
