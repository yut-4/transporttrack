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

    public DbSet<TrackingDevice> TrackingDevices { get; set; }

    public DbSet<TrackingSession> TrackingSessions { get; set; }

    public DbSet<LocationPing> LocationPings { get; set; }

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
            .HasOne(ruta => ruta.Conductor)
            .WithMany()
            .HasForeignKey(ruta => ruta.ConductorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Ruta>()
            .HasOne(ruta => ruta.Vehiculo)
            .WithMany()
            .HasForeignKey(ruta => ruta.VehiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrackingDevice>()
            .HasIndex(device => device.InstallationId)
            .IsUnique();

        modelBuilder.Entity<TrackingDevice>()
            .HasOne(device => device.Conductor)
            .WithMany()
            .HasForeignKey(device => device.ConductorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrackingSession>()
            .HasOne(session => session.Conductor)
            .WithMany()
            .HasForeignKey(session => session.ConductorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrackingSession>()
            .HasOne(session => session.Vehiculo)
            .WithMany()
            .HasForeignKey(session => session.VehiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrackingSession>()
            .HasOne(session => session.Ruta)
            .WithMany()
            .HasForeignKey(session => session.RutaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TrackingSession>()
            .HasOne(session => session.TrackingDevice)
            .WithMany()
            .HasForeignKey(session => session.TrackingDeviceId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<LocationPing>()
            .HasIndex(ping => ping.ClientPingId)
            .IsUnique();

        modelBuilder.Entity<LocationPing>()
            .HasIndex(ping => new { ping.ConductorId, ping.RecordedAtUtc });

        modelBuilder.Entity<LocationPing>()
            .HasIndex(ping => new { ping.TrackingSessionId, ping.RecordedAtUtc });

        modelBuilder.Entity<LocationPing>()
            .HasOne(ping => ping.TrackingSession)
            .WithMany()
            .HasForeignKey(ping => ping.TrackingSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LocationPing>()
            .HasOne(ping => ping.Conductor)
            .WithMany()
            .HasForeignKey(ping => ping.ConductorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
