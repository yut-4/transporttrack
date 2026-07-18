using Microsoft.AspNetCore.Mvc;
using TransportTrack.Application.Contract;
using TransportTrack.Application.Dtos.Vehicle;
using TransportTrack.Infrastructure.Exceptions;

namespace TransportTrack.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class VehiculosController(IVehicleService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAll() => Ok(await service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehicleDto>> GetById(int id)
    {
        var result = await service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<VehicleDto>> Create(CreateVehicleDto dto)
    {
        try
        {
            var result = await service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ConductorException exception) { return BadRequest(exception.Message); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateVehicleDto dto)
    {
        try { await service.UpdateAsync(id, dto); return NoContent(); }
        catch (VehiculoException exception) { return NotFound(exception.Message); }
        catch (ConductorException exception) { return BadRequest(exception.Message); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await service.DeleteAsync(id); return NoContent(); }
        catch (VehiculoException exception) { return NotFound(exception.Message); }
    }
}
