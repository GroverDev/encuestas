using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReglaEncuestaController : ControllerBase
{
    [HttpGet("{encuestaId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<ReglaEncuestaResponse>>>> ObtenerReglasEncuesta(Guid encuestaId)
    {
        var resp = await ReglaEncuestaBLL.ObtenerReglasEncuesta(encuestaId);
        return Ok(resp);
    }

    [HttpGet("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<ReglaEncuestaResponse?>>> ObtenerReglaEncuestaPorId(Guid encuestaId, Guid id)
    {
        var resp = await ReglaEncuestaBLL.ObtenerReglaEncuestaPorId(id, encuestaId);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearReglaEncuesta([FromBody] ReglaEncuestaRequest request)
    {
        var resp = await ReglaEncuestaBLL.CrearReglaEncuesta(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarReglaEncuesta([FromBody] ReglaEncuestaRequest request)
    {
        var resp = await ReglaEncuestaBLL.ActualizarReglaEncuesta(request);
        return Ok(resp);
    }

    [HttpDelete("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarReglaEncuesta(Guid encuestaId, Guid id)
    {
        var resp = await ReglaEncuestaBLL.EliminarReglaEncuesta(id, encuestaId);
        return Ok(resp);
    }
}
