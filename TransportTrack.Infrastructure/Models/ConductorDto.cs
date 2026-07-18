namespace TransportTrack.Infrastructure.Models;

public class ConductorDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Licencia { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;
}
