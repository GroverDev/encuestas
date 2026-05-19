using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SeccionEncuestaController : ControllerBase
{
    [HttpGet("{encuestaId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<SeccionEncuestaResponse>>>> ObtenerSecciones(Guid encuestaId)
    {
        var resp = await SeccionEncuestaBLL.ObtenerSecciones(encuestaId);
        return Ok(resp);
    }

    [HttpGet("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<SeccionEncuestaResponse?>>> ObtenerSeccionPorId(Guid encuestaId, Guid id)
    {
        var resp = await SeccionEncuestaBLL.ObtenerSeccionPorId(id, encuestaId);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearSeccion([FromBody] SeccionEncuestaRequest request)
    {
        var resp = await SeccionEncuestaBLL.CrearSeccion(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarSeccion([FromBody] SeccionEncuestaRequest request)
    {
        var resp = await SeccionEncuestaBLL.ActualizarSeccion(request);
        return Ok(resp);
    }

    [HttpDelete("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarSeccion(Guid encuestaId, Guid id)
    {
        var resp = await SeccionEncuestaBLL.EliminarSeccion(id, encuestaId);
        return Ok(resp);
    }
}
