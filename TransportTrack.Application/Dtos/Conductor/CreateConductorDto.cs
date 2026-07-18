using System.ComponentModel.DataAnnotations;

namespace TransportTrack.Application.Dtos.Conductor;

public class CreateConductorDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Nombre { get; init; } = string.Empty;
    [Required, StringLength(20, MinimumLength = 2)]
    public string Licencia { get; init; } = string.Empty;
    [Required, Phone, StringLength(20)]
    public string Telefono { get; init; } = string.Empty;
}
