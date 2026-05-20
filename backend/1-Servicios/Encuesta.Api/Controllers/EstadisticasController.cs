using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EstadisticasController : ControllerBase
{
    [HttpGet("{encuestaId}")]
    public async Task<ActionResult<Respuesta<EstadisticasEncuestaResponse>>> ObtenerEstadisticas(
        Guid encuestaId, [FromQuery] Guid? entidadId = null)
    {
        var resp = await EstadisticasBLL.ObtenerEstadisticas(encuestaId, entidadId);
        return Ok(resp);
    }

    [HttpGet("{encuestaId}/entidades")]
    public async Task<ActionResult<Respuesta<List<ResumenEntidadResponse>>>> ObtenerResumenPorEntidad(Guid encuestaId)
    {
        var resp = await EstadisticasBLL.ObtenerResumenPorEntidad(encuestaId);
        return Ok(resp);
    }
}
