using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PreguntaController : ControllerBase
{
    [HttpGet("{encuestaId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<PreguntaResponse>>>> ObtenerPreguntas(Guid encuestaId)
    {
        var resp = await PreguntaBLL.ObtenerPreguntas(encuestaId);
        return Ok(resp);
    }

    [HttpGet("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<PreguntaResponse?>>> ObtenerPreguntaPorId(Guid encuestaId, Guid id)
    {
        var resp = await PreguntaBLL.ObtenerPreguntaPorId(id, encuestaId);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearPregunta([FromBody] PreguntaRequest request)
    {
        var resp = await PreguntaBLL.CrearPregunta(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarPregunta([FromBody] PreguntaRequest request)
    {
        var resp = await PreguntaBLL.ActualizarPregunta(request);
        return Ok(resp);
    }

    [HttpDelete("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarPregunta(Guid encuestaId, Guid id)
    {
        var resp = await PreguntaBLL.EliminarPregunta(id, encuestaId);
        return Ok(resp);
    }
}
