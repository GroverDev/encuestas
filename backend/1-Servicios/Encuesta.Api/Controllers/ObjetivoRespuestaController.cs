using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ObjetivoRespuestaController : ControllerBase
{
    [HttpGet("{respuestaId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<ObjetivoRespuestaResponse>>>> ObtenerObjetivos(Guid respuestaId)
    {
        var resp = await ObjetivoRespuestaBLL.ObtenerObjetivos(respuestaId);
        return Ok(resp);
    }

    [HttpGet("{respuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<ObjetivoRespuestaResponse?>>> ObtenerObjetivoPorId(Guid respuestaId, Guid id)
    {
        var resp = await ObjetivoRespuestaBLL.ObtenerObjetivoPorId(id, respuestaId);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearObjetivo([FromBody] ObjetivoRespuestaRequest request)
    {
        var resp = await ObjetivoRespuestaBLL.CrearObjetivo(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarObjetivo([FromBody] ObjetivoRespuestaRequest request)
    {
        var resp = await ObjetivoRespuestaBLL.ActualizarObjetivo(request);
        return Ok(resp);
    }

    [HttpDelete("{respuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarObjetivo(Guid respuestaId, Guid id)
    {
        var resp = await ObjetivoRespuestaBLL.EliminarObjetivo(id, respuestaId);
        return Ok(resp);
    }
}
