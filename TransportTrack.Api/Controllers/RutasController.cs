using Microsoft.AspNetCore.Mvc;
using TransportTrack.Application.Contract;
using TransportTrack.Application.Dtos.Route;
using TransportTrack.Infrastructure.Exceptions;

namespace TransportTrack.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class RutasController(IRouteService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RouteDto>>> GetAll() => Ok(await service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RouteDto>> GetById(int id)
    {
        var result = await service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RouteDto>> Create(CreateRouteDto dto)
    {
        try
        {
            var result = await service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (RutaException exception) { return BadRequest(exception.Message); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateRouteDto dto)
    {
        try { await service.UpdateAsync(id, dto); return NoContent(); }
        catch (RutaException exception) { return BadRequest(exception.Message); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await service.DeleteAsync(id); return NoContent(); }
        catch (RutaException exception) { return NotFound(exception.Message); }
    }
}
