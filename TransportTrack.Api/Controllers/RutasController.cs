using Microsoft.AspNetCore.Mvc;
using TransportTrack.Application.Interfaces;
using TransportTrack.Domain.Entities;
using TransportTrack.Infrastructure.Exceptions;
using TransportTrack.Infrastructure.Models;

namespace TransportTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RutasController : ControllerBase
{
    private readonly IRutaService _rutaService;
    private readonly IConductorService _conductorService;
    private readonly IVehiculoService _vehiculoService;

    public RutasController(
        IRutaService rutaService,
        IConductorService conductorService,
        IVehiculoService vehiculoService)
    {
        _rutaService = rutaService;
        _conductorService = conductorService;
        _vehiculoService = vehiculoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RutaDto>>> GetRutas()
    {
        var rutas = await _rutaService.GetAllAsync();
        return Ok(rutas.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RutaDto>> GetRuta(int id)
    {
        var ruta = await _rutaService.GetByIdAsync(id);

        if (ruta is null)
        {
            return NotFound();
        }

        return Ok(ToDto(ruta));
    }

    [HttpPost]
    public async Task<ActionResult<RutaDto>> CrearRuta(RutaModel model)
    {
        try
        {
            var ruta = await _rutaService.CreateAsync(model);
            var rutaDto = ToDto(ruta);

            return CreatedAtAction(nameof(GetRuta), new { id = ruta.Id }, rutaDto);
        }
        catch (ConductorException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (VehiculoException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (RutaException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarRuta(int id, RutaModel model)
    {
        try
        {
            await _rutaService.UpdateAsync(id, model);
            return NoContent();
        }
        catch (RutaException exception)
        {
            return NotFound(exception.Message);
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarRuta(int id)
    {
        try
        {
            await _rutaService.DeleteAsync(id);
            return NoContent();
        }
        catch (RutaException exception)
        {
            return NotFound(exception.Message);
        }
    }

    private static RutaDto ToDto(Ruta ruta)
    {
        return new RutaDto
        {
            Id = ruta.Id,
            Origen = ruta.Origen,
            Destino = ruta.Destino,
            ConductorId = ruta.ConductorId,
            NombreConductor = ruta.Conductor?.Nombre,
            VehiculoId = ruta.VehiculoId,
            PlacaVehiculo = ruta.Vehiculo?.Placa,
            FechaSalida = ruta.FechaSalida,
            FechaLlegada = ruta.FechaLlegada
        };
    }
}
