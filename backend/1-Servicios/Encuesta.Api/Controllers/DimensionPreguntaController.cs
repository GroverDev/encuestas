using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DimensionPreguntaController : ControllerBase
{
    [HttpGet("{encuestaId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<DimensionPreguntaResponse>>>> ObtenerDimensiones(Guid encuestaId)
    {
        var resp = await DimensionPreguntaBLL.ObtenerDimensiones(encuestaId);
        return Ok(resp);
    }

    [HttpGet("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<DimensionPreguntaResponse?>>> ObtenerDimensionPorId(Guid encuestaId, Guid id)
    {
        var resp = await DimensionPreguntaBLL.ObtenerDimensionPorId(id, encuestaId);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearDimension([FromBody] DimensionPreguntaRequest request)
    {
        var resp = await DimensionPreguntaBLL.CrearDimension(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarDimension([FromBody] DimensionPreguntaRequest request)
    {
        var resp = await DimensionPreguntaBLL.ActualizarDimension(request);
        return Ok(resp);
    }

    [HttpDelete("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarDimension(Guid encuestaId, Guid id)
    {
        var resp = await DimensionPreguntaBLL.EliminarDimension(id, encuestaId);
        return Ok(resp);
    }
}
