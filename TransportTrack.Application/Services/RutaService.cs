using TransportTrack.Application.Core;
using TransportTrack.Application.Interfaces;
using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Core;
using TransportTrack.Infrastructure.Interfaces;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Application.Services;

public class RutaService : ServiceBase<Ruta>, IRutaService
{
    private readonly IRutaRepository _rutaRepository;

    public RutaService(IRutaRepository rutaRepository)
        : base(rutaRepository as BaseRepository<Ruta> ?? throw new InvalidOperationException("Invalid repository type"))
    {
        _rutaRepository = rutaRepository;
    }

    public async Task<Ruta> CreateAsync(RutaModel model)
    {
        return await _rutaRepository.AddAsync(model);
    }

    public async Task UpdateAsync(int id, RutaModel model)
    {
        await _rutaRepository.UpdateAsync(id, model);
    }

    async Task<Ruta?> IRutaService.GetByIdAsync(int id)
    {
        return await _rutaRepository.GetByIdAsync(id);
    }

    async Task<List<Ruta>> IRutaService.GetAllAsync()
    {
        return await _rutaRepository.GetAllAsync();
    }

    async Task IRutaService.DeleteAsync(int id)
    {
        await _rutaRepository.DeleteAsync(id);
    }
}
