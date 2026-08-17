using Microsoft.AspNetCore.Mvc;
using TransportTrack.Application.Interfaces;
using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Exceptions;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class ConductoresController(IConductorService service) : ControllerBase
{
    private readonly IConductorService _conductorService;

    public ConductoresController(IConductorService conductorService)
    {
        _conductorService = conductorService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConductorDto>>> GetConductores()
    {
        var conductores = await _conductorService.GetAllAsync();
        return Ok(conductores.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ConductorDto>> GetById(int id)
    {
        var conductor = await _conductorService.GetByIdAsync(id);

        if (conductor is null)
        {
            return NotFound();
        }

        return Ok(ToDto(conductor));
    }

    [HttpPost]
    public async Task<ActionResult<ConductorDto>> Create(CreateConductorDto dto)
    {
        try
        {
            var conductor = await _conductorService.CreateAsync(model);
            var conductorDto = ToDto(conductor);

            return CreatedAtAction(nameof(GetConductor), new { id = conductor.Id }, conductorDto);
        }
        catch (ConductorException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateConductorDto dto)
    {
        try
        {
            await _conductorService.UpdateAsync(id, model);
            return NoContent();
        }
        catch (ConductorException exception)
        {
            return NotFound(exception.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _conductorService.DeleteAsync(id);
            return NoContent();
        }
        catch (ConductorException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    private static ConductorDto ToDto(Conductor conductor)
    {
        return new ConductorDto
        {
            Id = conductor.Id,
            Nombre = conductor.Nombre,
            Licencia = conductor.Licencia,
            Telefono = conductor.Telefono
        };
    }
}
