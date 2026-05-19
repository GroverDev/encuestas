using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RespuestaController : ControllerBase
{
    [HttpGet("{encuestaId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<RespuestaResponse>>>> ObtenerRespuestas(Guid encuestaId)
    {
        var resp = await RespuestaBLL.ObtenerRespuestas(encuestaId);
        return Ok(resp);
    }

    [HttpGet("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<RespuestaResponse?>>> ObtenerRespuestaPorId(Guid encuestaId, Guid id)
    {
        var resp = await RespuestaBLL.ObtenerRespuestaPorId(id, encuestaId);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearRespuesta([FromBody] RespuestaRequest request)
    {
        var resp = await RespuestaBLL.CrearRespuesta(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarRespuesta([FromBody] RespuestaRequest request)
    {
        var resp = await RespuestaBLL.ActualizarRespuesta(request);
        return Ok(resp);
    }

    [HttpDelete("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarRespuesta(Guid encuestaId, Guid id)
    {
        var resp = await RespuestaBLL.EliminarRespuesta(id, encuestaId);
        return Ok(resp);
    }
}
