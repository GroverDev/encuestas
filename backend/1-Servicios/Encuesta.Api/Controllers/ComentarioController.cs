using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ComentarioController : ControllerBase
{
    [HttpGet("entidad/{entidadId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<ComentarioResponse>>>> ObtenerComentariosPorEntidad(Guid entidadId)
    {
        var resp = await ComentarioBLL.ObtenerComentariosPorEntidad(entidadId);
        return Ok(resp);
    }

    [HttpGet("respuesta/{respuestaId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<ComentarioResponse>>>> ObtenerComentariosPorRespuesta(Guid respuestaId)
    {
        var resp = await ComentarioBLL.ObtenerComentariosPorRespuesta(respuestaId);
        return Ok(resp);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Respuesta<ComentarioResponse?>>> ObtenerComentarioPorId(Guid id)
    {
        var resp = await ComentarioBLL.ObtenerComentarioPorId(id);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearComentario([FromBody] ComentarioRequest request)
    {
        var resp = await ComentarioBLL.CrearComentario(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarComentario([FromBody] ComentarioRequest request)
    {
        var resp = await ComentarioBLL.ActualizarComentario(request);
        return Ok(resp);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarComentario(Guid id)
    {
        var resp = await ComentarioBLL.EliminarComentario(id);
        return Ok(resp);
    }
}
