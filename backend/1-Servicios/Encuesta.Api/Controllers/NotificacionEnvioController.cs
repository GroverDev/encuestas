using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificacionEnvioController : ControllerBase
{
    [HttpGet("{invitacionId}")]
    public async Task<ActionResult<Respuesta<IEnumerable<NotificacionEnvioResponse>>>> ObtenerNotificaciones(Guid invitacionId)
    {
        var resp = await NotificacionEnvioBLL.ObtenerNotificaciones(invitacionId);
        return Ok(resp);
    }

    [HttpGet("{invitacionId}/{id}")]
    public async Task<ActionResult<Respuesta<NotificacionEnvioResponse?>>> ObtenerNotificacionPorId(Guid invitacionId, Guid id)
    {
        var resp = await NotificacionEnvioBLL.ObtenerNotificacionPorId(id, invitacionId);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> RegistrarNotificacion([FromBody] NotificacionEnvioRequest request)
    {
        var resp = await NotificacionEnvioBLL.RegistrarNotificacion(request);
        return Ok(resp);
    }
}
