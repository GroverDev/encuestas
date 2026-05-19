using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AlcanceEncuestaController : ControllerBase
{
    [HttpGet("{encuestaId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<AlcanceEncuestaResponse>>>> ObtenerAlcances(Guid encuestaId)
    {
        var resp = await AlcanceEncuestaBLL.ObtenerAlcances(encuestaId);
        return Ok(resp);
    }

    [HttpGet("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<AlcanceEncuestaResponse?>>> ObtenerAlcancePorId(Guid encuestaId, Guid id)
    {
        var resp = await AlcanceEncuestaBLL.ObtenerAlcancePorId(id, encuestaId);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearAlcance([FromBody] AlcanceEncuestaRequest request)
    {
        var resp = await AlcanceEncuestaBLL.CrearAlcance(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarAlcance([FromBody] AlcanceEncuestaRequest request)
    {
        var resp = await AlcanceEncuestaBLL.ActualizarAlcance(request);
        return Ok(resp);
    }

    [HttpDelete("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarAlcance(Guid encuestaId, Guid id)
    {
        var resp = await AlcanceEncuestaBLL.EliminarAlcance(id, encuestaId);
        return Ok(resp);
    }
}
