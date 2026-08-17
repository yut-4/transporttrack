using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Application.Interfaces;

public interface IRutaService
{
    Task<List<Ruta>> GetAllAsync();
    Task<Ruta?> GetByIdAsync(int id);
    Task<Ruta> CreateAsync(RutaModel model);
    Task UpdateAsync(int id, RutaModel model);
    Task DeleteAsync(int id);
}
