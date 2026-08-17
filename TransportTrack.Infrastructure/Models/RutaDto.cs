namespace TransportTrack.Infrastructure.Models;

public class RutaDto
{
    public int Id { get; set; }

    public string Origen { get; set; } = string.Empty;

    public string Destino { get; set; } = string.Empty;

    public int ConductorId { get; set; }

    public string? NombreConductor { get; set; }

    public int VehiculoId { get; set; }

    public string? PlacaVehiculo { get; set; }

    public DateTime FechaSalida { get; set; }

    public DateTime? FechaLlegada { get; set; }
}
