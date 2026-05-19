using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CuotaRespuestaController : ControllerBase
{
    [HttpGet("{encuestaId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<CuotaRespuestaResponse>>>> ObtenerCuotas(Guid encuestaId)
    {
        var resp = await CuotaRespuestaBLL.ObtenerCuotas(encuestaId);
        return Ok(resp);
    }

    [HttpGet("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<CuotaRespuestaResponse?>>> ObtenerCuotaPorId(Guid encuestaId, Guid id)
    {
        var resp = await CuotaRespuestaBLL.ObtenerCuotaPorId(id, encuestaId);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearCuota([FromBody] CuotaRespuestaRequest request)
    {
        var resp = await CuotaRespuestaBLL.CrearCuota(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarCuota([FromBody] CuotaRespuestaRequest request)
    {
        var resp = await CuotaRespuestaBLL.ActualizarCuota(request);
        return Ok(resp);
    }

    [HttpDelete("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarCuota(Guid encuestaId, Guid id)
    {
        var resp = await CuotaRespuestaBLL.EliminarCuota(id, encuestaId);
        return Ok(resp);
    }
}
