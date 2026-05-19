using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OpcionPreguntaController : ControllerBase
{
    [HttpGet("{preguntaId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<OpcionPreguntaResponse>>>> ObtenerOpciones(Guid preguntaId)
    {
        var resp = await OpcionPreguntaBLL.ObtenerOpciones(preguntaId);
        return Ok(resp);
    }

    [HttpGet("{preguntaId}/{id}")]
    public async Task<ActionResult<Respuesta<OpcionPreguntaResponse?>>> ObtenerOpcionPorId(Guid preguntaId, Guid id)
    {
        var resp = await OpcionPreguntaBLL.ObtenerOpcionPorId(id, preguntaId);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearOpcion([FromBody] OpcionPreguntaRequest request)
    {
        var resp = await OpcionPreguntaBLL.CrearOpcion(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarOpcion([FromBody] OpcionPreguntaRequest request)
    {
        var resp = await OpcionPreguntaBLL.ActualizarOpcion(request);
        return Ok(resp);
    }

    [HttpDelete("{preguntaId}/{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarOpcion(Guid preguntaId, Guid id)
    {
        var resp = await OpcionPreguntaBLL.EliminarOpcion(id, preguntaId);
        return Ok(resp);
    }
}
