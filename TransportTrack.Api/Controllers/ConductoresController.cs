using Microsoft.AspNetCore.Mvc;
using TransportTrack.Application.Contract;
using TransportTrack.Application.Dtos.Conductor;
using TransportTrack.Infrastructure.Exceptions;

namespace TransportTrack.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class ConductoresController(IConductorService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConductorDto>>> GetAll() => Ok(await service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ConductorDto>> GetById(int id)
    {
        var result = await service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ConductorDto>> Create(CreateConductorDto dto)
    {
        var result = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateConductorDto dto)
    {
        try { await service.UpdateAsync(id, dto); return NoContent(); }
        catch (ConductorException exception) { return NotFound(exception.Message); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try { await service.DeleteAsync(id); return NoContent(); }
        catch (ConductorException exception) { return BadRequest(exception.Message); }
    }
}
