using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Application.Interfaces;

public interface IConductorService
{
    Task<List<Conductor>> GetAllAsync();
    Task<Conductor?> GetByIdAsync(int id);
    Task<Conductor> CreateAsync(ConductorModel model);
    Task UpdateAsync(int id, ConductorModel model);
    Task DeleteAsync(int id);
}
