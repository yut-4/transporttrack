using Microsoft.EntityFrameworkCore;
using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Context;
using TransportTrack.Infrastructure.Core;
using TransportTrack.Infrastructure.Exceptions;
using TransportTrack.Infrastructure.Interfaces;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Infrastructure.Repositories;

public sealed class RutaRepository(TransportTrackContext context)
    : BaseRepository<Ruta>(context), IRutaRepository
{
    public override async Task<List<Ruta>> GetAllAsync() => await DbSet
        .Include(ruta => ruta.Vehiculo)
        .Where(ruta => !ruta.IsDeleted)
        .ToListAsync();

    public override async Task<Ruta?> GetByIdAsync(int id) => await DbSet
        .Include(ruta => ruta.Vehiculo)
        .FirstOrDefaultAsync(ruta => ruta.Id == id && !ruta.IsDeleted);

    public async Task<Ruta> AddAsync(RutaModel model)
    {
        await ValidateVehicleAsync(model.VehiculoId);
        var ruta = new Ruta { Origen = model.Origen, Destino = model.Destino,
            DistanciaKm = model.DistanciaKm, FechaSalida = model.FechaSalida, VehiculoId = model.VehiculoId };
        await base.AddAsync(ruta);
        return await GetByIdAsync(ruta.Id) ?? ruta;
    }

    public async Task UpdateAsync(int id, RutaModel model)
    {
        var ruta = await GetByIdAsync(id) ?? throw new RutaException("La ruta indicada no existe.");
        await ValidateVehicleAsync(model.VehiculoId);
        ruta.Origen = model.Origen; ruta.Destino = model.Destino; ruta.DistanciaKm = model.DistanciaKm;
        ruta.FechaSalida = model.FechaSalida; ruta.VehiculoId = model.VehiculoId;
        await base.UpdateAsync(ruta);
    }

    public async Task DeleteAsync(int id)
    {
        var ruta = await GetByIdAsync(id) ?? throw new RutaException("La ruta indicada no existe.");
        await base.DeleteAsync(ruta);
    }

    private async Task ValidateVehicleAsync(int id)
    {
        if (!await Context.Vehiculos.AnyAsync(v => v.Id == id && !v.IsDeleted))
            throw new RutaException("El vehiculo indicado no existe.");
    }
}
