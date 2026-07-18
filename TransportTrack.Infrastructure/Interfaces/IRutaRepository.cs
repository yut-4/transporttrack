using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Infrastructure.Interfaces;

public interface IRutaRepository
{
    Task<List<Ruta>> GetAllAsync();
    Task<Ruta?> GetByIdAsync(int id);
    Task<Ruta> AddAsync(RutaModel model);
    Task UpdateAsync(int id, RutaModel model);
    Task DeleteAsync(int id);
}
