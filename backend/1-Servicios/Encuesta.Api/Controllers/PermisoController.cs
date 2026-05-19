using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PermisoController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Respuesta<IEnumerable<PermisoResponse>>>> ObtenerPermisos()
    {
        var resp = await PermisoBLL.ObtenerPermisos();
        return Ok(resp);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Respuesta<PermisoResponse?>>> ObtenerPermisoPorId(Guid id)
    {
        var resp = await PermisoBLL.ObtenerPermisoPorId(id);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearPermiso([FromBody] PermisoRequest request)
    {
        var resp = await PermisoBLL.CrearPermiso(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarPermiso([FromBody] PermisoRequest request)
    {
        var resp = await PermisoBLL.ActualizarPermiso(request);
        return Ok(resp);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarPermiso(Guid id)
    {
        var resp = await PermisoBLL.EliminarPermiso(id);
        return Ok(resp);
    }
}
