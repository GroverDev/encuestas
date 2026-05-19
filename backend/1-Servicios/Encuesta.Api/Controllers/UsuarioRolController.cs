using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsuarioRolController : ControllerBase
{
    [HttpGet("{usuarioId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<UsuarioRolResponse>>>> ObtenerUsuarioRoles(Guid usuarioId)
    {
        var resp = await UsuarioRolBLL.ObtenerUsuarioRoles(usuarioId);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> AsignarRolUsuario([FromBody] UsuarioRolRequest request)
    {
        var resp = await UsuarioRolBLL.AsignarRolUsuario(request);
        return Ok(resp);
    }

    [HttpDelete("{usuarioId}/{rolId}")]
    public async Task<ActionResult<Respuesta<bool>>> RemoverRolUsuario(Guid usuarioId, Guid rolId)
    {
        var resp = await UsuarioRolBLL.RemoverRolUsuario(usuarioId, rolId);
        return Ok(resp);
    }
}
