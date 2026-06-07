using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using transporttrack.Context;
using transporttrack.DTOs;
using transporttrack.Models;

namespace transporttrack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConductoresController : ControllerBase
{
    private readonly TransportTrackContext _context;

    public ConductoresController(TransportTrackContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConductorDto>>> GetConductores()
    {
        var conductores = await _context.Conductores
            .Select(conductor => new ConductorDto
            {
                Id = conductor.Id,
                Nombre = conductor.Nombre,
                Licencia = conductor.Licencia,
                Telefono = conductor.Telefono
            })
            .ToListAsync();

        return Ok(conductores);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ConductorDto>> GetConductor(int id)
    {
        var conductor = await _context.Conductores.FindAsync(id);

        if (conductor is null)
        {
            return NotFound();
        }

        return Ok(new ConductorDto
        {
            Id = conductor.Id,
            Nombre = conductor.Nombre,
            Licencia = conductor.Licencia,
            Telefono = conductor.Telefono
        });
    }

    [HttpPost]
    public async Task<ActionResult<ConductorDto>> CrearConductor(CrearConductorDto crearConductorDto)
    {
        var conductor = new Conductor
        {
            Nombre = crearConductorDto.Nombre,
            Licencia = crearConductorDto.Licencia,
            Telefono = crearConductorDto.Telefono
        };

        _context.Conductores.Add(conductor);
        await _context.SaveChangesAsync();

        var conductorDto = new ConductorDto
        {
            Id = conductor.Id,
            Nombre = conductor.Nombre,
            Licencia = conductor.Licencia,
            Telefono = conductor.Telefono
        };

        return CreatedAtAction(nameof(GetConductor), new { id = conductor.Id }, conductorDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarConductor(int id, ActualizarConductorDto actualizarConductorDto)
    {
        var conductor = await _context.Conductores.FindAsync(id);

        if (conductor is null)
        {
            return NotFound();
        }

        conductor.Nombre = actualizarConductorDto.Nombre;
        conductor.Licencia = actualizarConductorDto.Licencia;
        conductor.Telefono = actualizarConductorDto.Telefono;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarConductor(int id)
    {
        var conductor = await _context.Conductores.FindAsync(id);

        if (conductor is null)
        {
            return NotFound();
        }

        var tieneVehiculos = await _context.Vehiculos.AnyAsync(vehiculo => vehiculo.ConductorId == id);

        if (tieneVehiculos)
        {
            return BadRequest("No se puede eliminar un conductor con vehiculos asignados.");
        }

        _context.Conductores.Remove(conductor);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
