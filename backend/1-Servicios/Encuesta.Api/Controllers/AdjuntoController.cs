using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AdjuntoController : ControllerBase
{
    [HttpGet("{entidadId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<AdjuntoResponse>>>> ObtenerAdjuntos(Guid entidadId)
    {
        var resp = await AdjuntoBLL.ObtenerAdjuntos(entidadId);
        return Ok(resp);
    }

    [HttpGet("{entidadId}/{id}")]
    public async Task<ActionResult<Respuesta<AdjuntoResponse?>>> ObtenerAdjuntoPorId(Guid entidadId, Guid id)
    {
        var resp = await AdjuntoBLL.ObtenerAdjuntoPorId(id, entidadId);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearAdjunto([FromBody] AdjuntoRequest request)
    {
        var resp = await AdjuntoBLL.CrearAdjunto(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarAdjunto([FromBody] AdjuntoRequest request)
    {
        var resp = await AdjuntoBLL.ActualizarAdjunto(request);
        return Ok(resp);
    }

    [HttpDelete("{entidadId}/{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarAdjunto(Guid entidadId, Guid id)
    {
        var resp = await AdjuntoBLL.EliminarAdjunto(id, entidadId);
        return Ok(resp);
    }
}
