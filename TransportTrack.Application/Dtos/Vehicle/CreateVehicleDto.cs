using System.ComponentModel.DataAnnotations;

namespace TransportTrack.Application.Dtos.Vehicle;

public class CreateVehicleDto
{
    [Required, StringLength(20, MinimumLength = 3)]
    public string Placa { get; init; } = string.Empty;
    [Required, StringLength(60, MinimumLength = 2)]
    public string Marca { get; init; } = string.Empty;
    [Required, StringLength(60, MinimumLength = 1)]
    public string Modelo { get; init; } = string.Empty;
    [Range(1900, 2100)]
    public int Anio { get; init; }
    [Range(1, int.MaxValue)]
    public int ConductorId { get; init; }
}
