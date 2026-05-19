using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TipoEntidadController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Respuesta<IEnumerable<TipoEntidadResponse>>>> ObtenerTiposEntidad()
    {
        var resp = await TipoEntidadBLL.ObtenerTiposEntidad();
        return Ok(resp);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Respuesta<TipoEntidadResponse?>>> ObtenerTipoEntidadPorId(Guid id)
    {
        var resp = await TipoEntidadBLL.ObtenerTipoEntidadPorId(id);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearTipoEntidad([FromBody] TipoEntidadRequest request)
    {
        var resp = await TipoEntidadBLL.CrearTipoEntidad(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarTipoEntidad([FromBody] TipoEntidadRequest request)
    {
        var resp = await TipoEntidadBLL.ActualizarTipoEntidad(request);
        return Ok(resp);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarTipoEntidad(Guid id)
    {
        var resp = await TipoEntidadBLL.EliminarTipoEntidad(id);
        return Ok(resp);
    }
}
