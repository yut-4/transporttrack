using System.ComponentModel.DataAnnotations;
using TransportTrack.Domain.Core;

namespace TransportTrack.Domain.Entities;

public class Ruta : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Origen { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Destino { get; set; } = string.Empty;

    public int ConductorId { get; set; }

    public Conductor? Conductor { get; set; }

    public int VehiculoId { get; set; }

    public Vehiculo? Vehiculo { get; set; }

    public DateTime FechaSalida { get; set; } = DateTime.Now;

    public DateTime? FechaLlegada { get; set; }

    public Ruta() : base()
    {
    }

    public Ruta(string origen, string destino, int conductorId, int vehiculoId) : base()
    {
        Origen = origen;
        Destino = destino;
        ConductorId = conductorId;
        VehiculoId = vehiculoId;
    }

    public Ruta(int id, string origen, string destino, int conductorId, int vehiculoId, DateTime fechaSalida, DateTime? fechaLlegada = null, DateTime? createdAt = null, bool isDeleted = false)
        : base(createdAt ?? DateTime.Now)
    {
        Id = id;
        Origen = origen;
        Destino = destino;
        ConductorId = conductorId;
        VehiculoId = vehiculoId;
        FechaSalida = fechaSalida;
        FechaLlegada = fechaLlegada;
        IsDeleted = isDeleted;
    }
}
