using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ColaboradorEncuestaController : ControllerBase
{
    [HttpGet("{encuestaId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<ColaboradorEncuestaResponse>>>> ObtenerColaboradores(Guid encuestaId)
    {
        var resp = await ColaboradorEncuestaBLL.ObtenerColaboradores(encuestaId);
        return Ok(resp);
    }

    [HttpGet("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<ColaboradorEncuestaResponse?>>> ObtenerColaboradorPorId(Guid encuestaId, Guid id)
    {
        var resp = await ColaboradorEncuestaBLL.ObtenerColaboradorPorId(id, encuestaId);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearColaborador([FromBody] ColaboradorEncuestaRequest request)
    {
        var resp = await ColaboradorEncuestaBLL.CrearColaborador(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarColaborador([FromBody] ColaboradorEncuestaRequest request)
    {
        var resp = await ColaboradorEncuestaBLL.ActualizarColaborador(request);
        return Ok(resp);
    }

    [HttpDelete("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarColaborador(Guid encuestaId, Guid id)
    {
        var resp = await ColaboradorEncuestaBLL.EliminarColaborador(id, encuestaId);
        return Ok(resp);
    }
}
