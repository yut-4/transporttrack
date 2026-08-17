using Microsoft.EntityFrameworkCore;
using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Context;
using TransportTrack.Infrastructure.Core;
using TransportTrack.Infrastructure.Exceptions;
using TransportTrack.Infrastructure.Interfaces;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Infrastructure.Repositories;

public class RutaRepository : BaseRepository<Ruta>, IRutaRepository
{
    public RutaRepository(TransportTrackContext context) : base(context)
    {
    }

    public override async Task<List<Ruta>> GetAllAsync()
    {
        return await DbSet
            .Include(ruta => ruta.Conductor)
            .Include(ruta => ruta.Vehiculo)
            .Where(ruta => !ruta.IsDeleted)
            .ToListAsync();
    }

    public override async Task<Ruta?> GetByIdAsync(int id)
    {
        return await DbSet
            .Include(ruta => ruta.Conductor)
            .Include(ruta => ruta.Vehiculo)
            .FirstOrDefaultAsync(ruta => ruta.Id == id && !ruta.IsDeleted);
    }

    public async Task<Ruta> AddAsync(RutaModel model)
    {
        await ValidateConductorAsync(model.ConductorId);
        await ValidateVehiculoAsync(model.VehiculoId);

        var ruta = new Ruta(
            model.Origen,
            model.Destino,
            model.ConductorId,
            model.VehiculoId)
        {
            FechaSalida = model.FechaSalida ?? DateTime.Now,
            FechaLlegada = model.FechaLlegada
        };

        await base.AddAsync(ruta);

        return await GetByIdAsync(ruta.Id) ?? ruta;
    }

    public async Task UpdateAsync(int id, RutaModel model)
    {
        var ruta = await GetByIdAsync(id);

        if (ruta is null)
        {
            throw new RutaException("La ruta indicada no existe.");
        }

        await ValidateConductorAsync(model.ConductorId);
        await ValidateVehiculoAsync(model.VehiculoId);

        ruta.Origen = model.Origen;
        ruta.Destino = model.Destino;
        ruta.ConductorId = model.ConductorId;
        ruta.VehiculoId = model.VehiculoId;
        ruta.FechaSalida = model.FechaSalida ?? ruta.FechaSalida;
        ruta.FechaLlegada = model.FechaLlegada;

        await base.UpdateAsync(ruta);
    }

    public async Task DeleteAsync(int id)
    {
        var ruta = await GetByIdAsync(id);

        if (ruta is null)
        {
            throw new RutaException("La ruta indicada no existe.");
        }

        await base.DeleteAsync(ruta);
    }

    private async Task ValidateConductorAsync(int conductorId)
    {
        var existeConductor = await Context.Conductores
            .AnyAsync(conductor => conductor.Id == conductorId && !conductor.IsDeleted);

        if (!existeConductor)
        {
            throw new ConductorException("El conductor indicado no existe.");
        }
    }

    private async Task ValidateVehiculoAsync(int vehiculoId)
    {
        var existeVehiculo = await Context.Vehiculos
            .AnyAsync(vehiculo => vehiculo.Id == vehiculoId && !vehiculo.IsDeleted);

        if (!existeVehiculo)
        {
            throw new VehiculoException("El vehículo indicado no existe.");
        }
    }
}
