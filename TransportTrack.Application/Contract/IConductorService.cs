using TransportTrack.Application.Dtos.Conductor;

namespace TransportTrack.Application.Contract;

public interface IConductorService
{
    Task<IReadOnlyList<ConductorDto>> GetAllAsync();
    Task<ConductorDto?> GetByIdAsync(int id);
    Task<ConductorDto> CreateAsync(CreateConductorDto dto);
    Task UpdateAsync(int id, UpdateConductorDto dto);
    Task DeleteAsync(int id);
}
