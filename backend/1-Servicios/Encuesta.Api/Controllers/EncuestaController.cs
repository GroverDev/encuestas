using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EncuestaController : ControllerBase
{
    private Guid GetOrganizacionId() =>
        Guid.TryParse(User.Claims.FirstOrDefault(c => c.Type == "OrganizacionId")?.Value, out var id) ? id : Guid.Empty;

    private Guid GetUsuarioId() =>
        Guid.TryParse(User.Claims.FirstOrDefault(c => c.Type == "UsuarioId")?.Value, out var id) ? id : Guid.Empty;

    [HttpGet]
    public async Task<ActionResult<Respuesta<IEnumerable<EncuestaResponse>>>> ObtenerEncuestas()
    {
        var resp = await EncuestaBLL.ObtenerEncuestas(GetOrganizacionId());
        return Ok(resp);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Respuesta<EncuestaResponse?>>> ObtenerEncuestaPorId(Guid id)
    {
        var resp = await EncuestaBLL.ObtenerEncuestaPorId(id, GetOrganizacionId());
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearEncuesta([FromBody] EncuestaRequest request)
    {
        request.OrganizacionId = GetOrganizacionId();
        request.CreadoPorUsuarioId = GetUsuarioId();
        var resp = await EncuestaBLL.CrearEncuesta(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarEncuesta([FromBody] EncuestaRequest request)
    {
        request.OrganizacionId = GetOrganizacionId();
        var resp = await EncuestaBLL.ActualizarEncuesta(request);
        return Ok(resp);
    }

    [HttpPatch("{id}/estado")]
    public async Task<ActionResult<Respuesta<bool>>> CambiarEstado(Guid id, [FromBody] CambiarEstadoEncuestaRequest request)
    {
        request.Id = id;
        var resp = await EncuestaBLL.CambiarEstado(id, GetOrganizacionId(), request.NuevoEstado);
        return Ok(resp);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarEncuesta(Guid id)
    {
        var resp = await EncuestaBLL.EliminarEncuesta(id, GetOrganizacionId());
        return Ok(resp);
    }
}
