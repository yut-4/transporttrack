using Microsoft.AspNetCore.Mvc;
using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Exceptions;
using TransportTrack.Infrastructure.Interfaces;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConductoresController : ControllerBase
{
    private readonly IConductorRepository _conductorRepository;

    public ConductoresController(IConductorRepository conductorRepository)
    {
        _conductorRepository = conductorRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConductorDto>>> GetConductores()
    {
        var conductores = await _conductorRepository.GetAllAsync();
        return Ok(conductores.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ConductorDto>> GetConductor(int id)
    {
        var conductor = await _conductorRepository.GetByIdAsync(id);

        if (conductor is null)
        {
            return NotFound();
        }

        return Ok(ToDto(conductor));
    }

    [HttpPost]
    public async Task<ActionResult<ConductorDto>> CrearConductor(ConductorModel model)
    {
        var conductor = await _conductorRepository.AddAsync(model);
        var conductorDto = ToDto(conductor);

        return CreatedAtAction(nameof(GetConductor), new { id = conductor.Id }, conductorDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarConductor(int id, ConductorModel model)
    {
        try
        {
            await _conductorRepository.UpdateAsync(id, model);
            return NoContent();
        }
        catch (ConductorException exception)
        {
            return NotFound(exception.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarConductor(int id)
    {
        try
        {
            await _conductorRepository.DeleteAsync(id);
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
