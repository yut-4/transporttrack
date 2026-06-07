using System.ComponentModel.DataAnnotations;

namespace transporttrack.Models;

public class Vehiculo
{
    public int Id { get; set; }

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
}
