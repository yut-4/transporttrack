using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Infrastructure.Interfaces;

public interface IConductorRepository
{
    Task<List<Conductor>> GetAllAsync();

    Task<Conductor?> GetByIdAsync(int id);

    Task<Conductor> AddAsync(ConductorModel model);

    Task UpdateAsync(int id, ConductorModel model);

    Task DeleteAsync(int id);
}
