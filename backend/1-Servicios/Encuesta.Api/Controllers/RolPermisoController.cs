using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RolPermisoController : ControllerBase
{
    [HttpGet("{rolId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<RolPermisoResponse>>>> ObtenerRolPermisos(Guid rolId)
    {
        var resp = await RolPermisoBLL.ObtenerRolPermisos(rolId);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> AsignarPermisoRol([FromBody] RolPermisoRequest request)
    {
        var resp = await RolPermisoBLL.AsignarPermisoRol(request);
        return Ok(resp);
    }

    [HttpDelete("{rolId}/{permisoId}")]
    public async Task<ActionResult<Respuesta<bool>>> RemoverPermisoRol(Guid rolId, Guid permisoId)
    {
        var resp = await RolPermisoBLL.RemoverPermisoRol(rolId, permisoId);
        return Ok(resp);
    }
}
