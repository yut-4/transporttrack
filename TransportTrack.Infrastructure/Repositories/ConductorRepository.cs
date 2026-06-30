using Microsoft.EntityFrameworkCore;
using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Context;
using TransportTrack.Infrastructure.Core;
using TransportTrack.Infrastructure.Exceptions;
using TransportTrack.Infrastructure.Interfaces;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Infrastructure.Repositories;

public class ConductorRepository : BaseRepository<Conductor>, IConductorRepository
{
    public ConductorRepository(TransportTrackContext context) : base(context)
    {
    }

    public async Task<Conductor> AddAsync(ConductorModel model)
    {
        var conductor = new Conductor
        {
            Nombre = model.Nombre,
            Licencia = model.Licencia,
            Telefono = model.Telefono
        };

        await base.AddAsync(conductor);
        return conductor;
    }

    public async Task UpdateAsync(int id, ConductorModel model)
    {
        var conductor = await GetByIdAsync(id);

        if (conductor is null)
        {
            throw new ConductorException("El conductor indicado no existe.");
        }

        conductor.Nombre = model.Nombre;
        conductor.Licencia = model.Licencia;
        conductor.Telefono = model.Telefono;

        await base.UpdateAsync(conductor);
    }

    public async Task DeleteAsync(int id)
    {
        var conductor = await GetByIdAsync(id);

        if (conductor is null)
        {
            throw new ConductorException("El conductor indicado no existe.");
        }

        var tieneVehiculos = await Context.Vehiculos
            .AnyAsync(vehiculo => vehiculo.ConductorId == id && !vehiculo.IsDeleted);

        if (tieneVehiculos)
        {
            throw new ConductorException("No se puede eliminar un conductor con vehiculos asignados.");
        }

        await base.DeleteAsync(conductor);
    }
}
