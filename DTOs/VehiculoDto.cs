namespace transporttrack.DTOs;

public class VehiculoDto
{
    public int Id { get; set; }

    public string Placa { get; set; } = string.Empty;

    public string Marca { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public int Anio { get; set; }

    public int ConductorId { get; set; }

    public string? NombreConductor { get; set; }
}
