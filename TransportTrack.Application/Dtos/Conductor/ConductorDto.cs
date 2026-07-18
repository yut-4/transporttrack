namespace TransportTrack.Application.Dtos.Conductor;

public sealed class ConductorDto
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Licencia { get; init; } = string.Empty;
    public string Telefono { get; init; } = string.Empty;
}
