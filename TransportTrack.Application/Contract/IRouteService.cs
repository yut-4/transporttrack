using TransportTrack.Application.Dtos.Route;

namespace TransportTrack.Application.Contract;

public interface IRouteService
{
    Task<IReadOnlyList<RouteDto>> GetAllAsync();
    Task<RouteDto?> GetByIdAsync(int id);
    Task<RouteDto> CreateAsync(CreateRouteDto dto);
    Task UpdateAsync(int id, UpdateRouteDto dto);
    Task DeleteAsync(int id);
}
