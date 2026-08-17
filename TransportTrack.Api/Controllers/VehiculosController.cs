using Microsoft.AspNetCore.Mvc;
using TransportTrack.Application.Interfaces;
using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Exceptions;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiculosController : ControllerBase
{
    private readonly IVehiculoService _vehiculoService;

    public VehiculosController(IVehiculoService vehiculoService)
    {
        _vehiculoService = vehiculoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehiculoDto>>> GetVehiculos()
    {
        var vehiculos = await _vehiculoService.GetAllAsync();
        return Ok(vehiculos.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VehiculoDto>> GetVehiculo(int id)
    {
        var vehiculo = await _vehiculoService.GetByIdAsync(id);

        if (vehiculo is null)
        {
            return NotFound();
        }

        return Ok(ToDto(vehiculo));
    }

    [HttpPost]
    public async Task<ActionResult<VehiculoDto>> CrearVehiculo(VehiculoModel model)
    {
        try
        {
            var vehiculo = await _vehiculoService.CreateAsync(model);
            var vehiculoDto = ToDto(vehiculo);

            return CreatedAtAction(nameof(GetVehiculo), new { id = vehiculo.Id }, vehiculoDto);
        }
        catch (ConductorException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (VehiculoException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarVehiculo(int id, VehiculoModel model)
    {
        try
        {
            await _vehiculoService.UpdateAsync(id, model);
            return NoContent();
        }
        catch (VehiculoException exception)
        {
            return NotFound(exception.Message);
        }
        catch (ConductorException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarVehiculo(int id)
    {
        try
        {
            await _vehiculoService.DeleteAsync(id);
            return NoContent();
        }
        catch (VehiculoException exception)
        {
            return NotFound(exception.Message);
        }
    }

    private static VehiculoDto ToDto(Vehiculo vehiculo)
    {
        return new VehiculoDto
        {
            Id = vehiculo.Id,
            Placa = vehiculo.Placa,
            Marca = vehiculo.Marca,
            Modelo = vehiculo.Modelo,
            Anio = vehiculo.Anio,
            ConductorId = vehiculo.ConductorId,
            NombreConductor = vehiculo.Conductor?.Nombre
        };
    }
}
