using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class InvitacionController : ControllerBase
{
    private Guid GetOrganizacionId() =>
        Guid.TryParse(User.Claims.FirstOrDefault(c => c.Type == "OrganizacionId")?.Value, out var id) ? id : Guid.Empty;

    [HttpGet("{encuestaId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<InvitacionResponse>>>> ObtenerInvitaciones(Guid encuestaId)
    {
        var resp = await InvitacionBLL.ObtenerInvitaciones(encuestaId);
        return Ok(resp);
    }

    [HttpGet("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<InvitacionResponse?>>> ObtenerInvitacionPorId(Guid encuestaId, Guid id)
    {
        var resp = await InvitacionBLL.ObtenerInvitacionPorId(id, encuestaId);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<Guid>>> CrearInvitacion([FromBody] InvitacionRequest request)
    {
        var resp = await InvitacionBLL.CrearInvitacion(request, GetOrganizacionId());
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarInvitacion([FromBody] InvitacionRequest request)
    {
        var resp = await InvitacionBLL.ActualizarInvitacion(request);
        return Ok(resp);
    }

    [HttpDelete("{encuestaId}/{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarInvitacion(Guid encuestaId, Guid id)
    {
        var resp = await InvitacionBLL.EliminarInvitacion(id, encuestaId);
        return Ok(resp);
    }
}
