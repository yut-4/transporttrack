using Microsoft.EntityFrameworkCore;
using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Context;
using TransportTrack.Infrastructure.Core;
using TransportTrack.Infrastructure.Exceptions;
using TransportTrack.Infrastructure.Interfaces;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Infrastructure.Repositories;

public class VehiculoRepository : BaseRepository<Vehiculo>, IVehiculoRepository
{
    public VehiculoRepository(TransportTrackContext context) : base(context)
    {
    }

    public override async Task<List<Vehiculo>> GetAllAsync()
    {
        return await DbSet
            .Include(vehiculo => vehiculo.Conductor)
            .Where(vehiculo => !vehiculo.IsDeleted)
            .ToListAsync();
    }

    public override async Task<Vehiculo?> GetByIdAsync(int id)
    {
        return await DbSet
            .Include(vehiculo => vehiculo.Conductor)
            .FirstOrDefaultAsync(vehiculo => vehiculo.Id == id && !vehiculo.IsDeleted);
    }

    public async Task<Vehiculo> AddAsync(VehiculoModel model)
    {
        await ValidateConductorAsync(model.ConductorId);

        var vehiculo = new Vehiculo
        {
            Placa = model.Placa,
            Marca = model.Marca,
            Modelo = model.Modelo,
            Anio = model.Anio,
            ConductorId = model.ConductorId
        };

        await base.AddAsync(vehiculo);

        return await GetByIdAsync(vehiculo.Id) ?? vehiculo;
    }

    public async Task UpdateAsync(int id, VehiculoModel model)
    {
        var vehiculo = await GetByIdAsync(id);

        if (vehiculo is null)
        {
            throw new VehiculoException("El vehiculo indicado no existe.");
        }

        await ValidateConductorAsync(model.ConductorId);

        vehiculo.Placa = model.Placa;
        vehiculo.Marca = model.Marca;
        vehiculo.Modelo = model.Modelo;
        vehiculo.Anio = model.Anio;
        vehiculo.ConductorId = model.ConductorId;

        await base.UpdateAsync(vehiculo);
    }

    public async Task DeleteAsync(int id)
    {
        var vehiculo = await GetByIdAsync(id);

        if (vehiculo is null)
        {
            throw new VehiculoException("El vehiculo indicado no existe.");
        }

        await base.DeleteAsync(vehiculo);
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
}
