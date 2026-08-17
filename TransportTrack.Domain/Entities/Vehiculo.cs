using System.ComponentModel.DataAnnotations;
using TransportTrack.Domain.Core;

namespace TransportTrack.Domain.Entities;

public class Vehiculo : BaseEntity
{
    [Required]
    [MaxLength(20)]
    public string Placa { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    public string Marca { get; set; } = string.Empty;

    [Required]
    [MaxLength(60)]
    public string Modelo { get; set; } = string.Empty;

    public int Anio { get; set; }

    public int ConductorId { get; set; }

    public Conductor? Conductor { get; set; }

    public Vehiculo() : base()
    {
    }

    public Vehiculo(string placa, string marca, string modelo, int anio, int conductorId) : base()
    {
        Placa = placa;
        Marca = marca;
        Modelo = modelo;
        Anio = anio;
        ConductorId = conductorId;
    }

    public Vehiculo(int id, string placa, string marca, string modelo, int anio, int conductorId, DateTime createdAt, bool isDeleted = false)
        : base(createdAt)
    {
        Id = id;
        Placa = placa;
        Marca = marca;
        Modelo = modelo;
        Anio = anio;
        ConductorId = conductorId;
        IsDeleted = isDeleted;
    }
}
