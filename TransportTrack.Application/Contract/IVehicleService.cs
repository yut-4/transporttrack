using TransportTrack.Application.Dtos.Vehicle;

namespace TransportTrack.Application.Contract;

public interface IVehicleService
{
    Task<IReadOnlyList<VehicleDto>> GetAllAsync();
    Task<VehicleDto?> GetByIdAsync(int id);
    Task<VehicleDto> CreateAsync(CreateVehicleDto dto);
    Task UpdateAsync(int id, UpdateVehicleDto dto);
    Task DeleteAsync(int id);
}
