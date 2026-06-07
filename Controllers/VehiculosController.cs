using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using transporttrack.Context;
using transporttrack.DTOs;
using transporttrack.Models;

namespace transporttrack.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiculosController : ControllerBase
{
    private readonly TransportTrackContext _context;

    public VehiculosController(TransportTrackContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehiculoDto>>> GetVehiculos()
    {
        var vehiculos = await _context.Vehiculos
            .Include(vehiculo => vehiculo.Conductor)
            .Select(vehiculo => new VehiculoDto
            {
                Id = vehiculo.Id,
                Placa = vehiculo.Placa,
                Marca = vehiculo.Marca,
                Modelo = vehiculo.Modelo,
                Anio = vehiculo.Anio,
                ConductorId = vehiculo.ConductorId,
                NombreConductor = vehiculo.Conductor == null ? null : vehiculo.Conductor.Nombre
            })
            .ToListAsync();

        return Ok(vehiculos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehiculoDto>> GetVehiculo(int id)
    {
        var vehiculo = await _context.Vehiculos
            .Include(item => item.Conductor)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (vehiculo is null)
        {
            return NotFound();
        }

        return Ok(new VehiculoDto
        {
            Id = vehiculo.Id,
            Placa = vehiculo.Placa,
            Marca = vehiculo.Marca,
            Modelo = vehiculo.Modelo,
            Anio = vehiculo.Anio,
            ConductorId = vehiculo.ConductorId,
            NombreConductor = vehiculo.Conductor?.Nombre
        });
    }

    [HttpPost]
    public async Task<ActionResult<VehiculoDto>> CrearVehiculo(CrearVehiculoDto crearVehiculoDto)
    {
        var existeConductor = await _context.Conductores.AnyAsync(conductor => conductor.Id == crearVehiculoDto.ConductorId);

        if (!existeConductor)
        {
            return BadRequest("El conductor indicado no existe.");
        }

        var vehiculo = new Vehiculo
        {
            Placa = crearVehiculoDto.Placa,
            Marca = crearVehiculoDto.Marca,
            Modelo = crearVehiculoDto.Modelo,
            Anio = crearVehiculoDto.Anio,
            ConductorId = crearVehiculoDto.ConductorId
        };

        _context.Vehiculos.Add(vehiculo);
        await _context.SaveChangesAsync();

        var vehiculoDto = await _context.Vehiculos
            .Include(item => item.Conductor)
            .Where(item => item.Id == vehiculo.Id)
            .Select(item => new VehiculoDto
            {
                Id = item.Id,
                Placa = item.Placa,
                Marca = item.Marca,
                Modelo = item.Modelo,
                Anio = item.Anio,
                ConductorId = item.ConductorId,
                NombreConductor = item.Conductor == null ? null : item.Conductor.Nombre
            })
            .FirstAsync();

        return CreatedAtAction(nameof(GetVehiculo), new { id = vehiculo.Id }, vehiculoDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarVehiculo(int id, ActualizarVehiculoDto actualizarVehiculoDto)
    {
        var vehiculo = await _context.Vehiculos.FindAsync(id);

        if (vehiculo is null)
        {
            return NotFound();
        }

        var existeConductor = await _context.Conductores.AnyAsync(conductor => conductor.Id == actualizarVehiculoDto.ConductorId);

        if (!existeConductor)
        {
            return BadRequest("El conductor indicado no existe.");
        }

        vehiculo.Placa = actualizarVehiculoDto.Placa;
        vehiculo.Marca = actualizarVehiculoDto.Marca;
        vehiculo.Modelo = actualizarVehiculoDto.Modelo;
        vehiculo.Anio = actualizarVehiculoDto.Anio;
        vehiculo.ConductorId = actualizarVehiculoDto.ConductorId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarVehiculo(int id)
    {
        var vehiculo = await _context.Vehiculos.FindAsync(id);

        if (vehiculo is null)
        {
            return NotFound();
        }

        _context.Vehiculos.Remove(vehiculo);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
