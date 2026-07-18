using Microsoft.EntityFrameworkCore;
using TransportTrack.Domain.Entities;

namespace TransportTrack.Infrastructure.Context;

public class TransportTrackContext : DbContext
{
    public TransportTrackContext(DbContextOptions<TransportTrackContext> options)
        : base(options)
    {
    }

    public DbSet<Conductor> Conductores { get; set; }

    public DbSet<Vehiculo> Vehiculos { get; set; }

    public DbSet<Ruta> Rutas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Conductor>()
            .HasIndex(conductor => conductor.Licencia)
            .IsUnique();

        modelBuilder.Entity<Vehiculo>()
            .HasIndex(vehiculo => vehiculo.Placa)
            .IsUnique();

        modelBuilder.Entity<Vehiculo>()
            .HasOne(vehiculo => vehiculo.Conductor)
            .WithMany(conductor => conductor.Vehiculos)
            .HasForeignKey(vehiculo => vehiculo.ConductorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ruta>()
            .HasOne(ruta => ruta.Vehiculo)
            .WithMany()
            .HasForeignKey(ruta => ruta.VehiculoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
