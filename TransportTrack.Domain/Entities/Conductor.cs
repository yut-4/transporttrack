using System.ComponentModel.DataAnnotations;
using TransportTrack.Domain.Core;

namespace TransportTrack.Domain.Entities;

public class Conductor : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Licencia { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Telefono { get; set; } = string.Empty;

    public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();

    public Conductor() : base()
    {
    }

    public Conductor(string nombre, string licencia, string telefono = "") : base()
    {
        Nombre = nombre;
        Licencia = licencia;
        Telefono = telefono;
    }

    public Conductor(int id, string nombre, string licencia, string telefono, DateTime createdAt, bool isDeleted = false)
        : base(createdAt)
    {
        Id = id;
        Nombre = nombre;
        Licencia = licencia;
        Telefono = telefono;
        IsDeleted = isDeleted;
    }
}
