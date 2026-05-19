using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DetalleRespuestaController : ControllerBase
{
    [HttpGet("{respuestaId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<DetalleRespuestaResponse>>>> ObtenerDetallesRespuesta(Guid respuestaId)
    {
        var resp = await DetalleRespuestaBLL.ObtenerDetallesRespuesta(respuestaId);
        return Ok(resp);
    }

    [HttpGet("{respuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<DetalleRespuestaResponse?>>> ObtenerDetalleRespuestaPorId(Guid respuestaId, Guid id)
    {
        var resp = await DetalleRespuestaBLL.ObtenerDetalleRespuestaPorId(id, respuestaId);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearDetalleRespuesta([FromBody] DetalleRespuestaRequest request)
    {
        var resp = await DetalleRespuestaBLL.CrearDetalleRespuesta(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarDetalleRespuesta([FromBody] DetalleRespuestaRequest request)
    {
        var resp = await DetalleRespuestaBLL.ActualizarDetalleRespuesta(request);
        return Ok(resp);
    }

    [HttpDelete("{respuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarDetalleRespuesta(Guid respuestaId, Guid id)
    {
        var resp = await DetalleRespuestaBLL.EliminarDetalleRespuesta(id, respuestaId);
        return Ok(resp);
    }
}
