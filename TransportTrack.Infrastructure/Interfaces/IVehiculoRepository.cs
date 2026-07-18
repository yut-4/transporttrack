using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Infrastructure.Interfaces;

public interface IVehiculoRepository
{
    Task<List<Vehiculo>> GetAllAsync();

    Task<Vehiculo?> GetByIdAsync(int id);

    Task<Vehiculo> AddAsync(VehiculoModel model);

    Task UpdateAsync(int id, VehiculoModel model);

    Task DeleteAsync(int id);
}
