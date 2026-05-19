using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RolController : ControllerBase
{
    private Guid GetOrganizacionId() =>
        Guid.TryParse(User.Claims.FirstOrDefault(c => c.Type == "OrganizacionId")?.Value, out var id) ? id : Guid.Empty;

    [HttpGet]
    public async Task<ActionResult<Respuesta<IEnumerable<RolResponse>>>> ObtenerRoles()
    {
        var resp = await RolBLL.ObtenerRoles(GetOrganizacionId());
        return Ok(resp);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Respuesta<RolResponse?>>> ObtenerRolPorId(Guid id)
    {
        var resp = await RolBLL.ObtenerRolPorId(id, GetOrganizacionId());
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearRol([FromBody] RolRequest request)
    {
        request.OrganizacionId = GetOrganizacionId();
        var resp = await RolBLL.CrearRol(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarRol([FromBody] RolRequest request)
    {
        request.OrganizacionId = GetOrganizacionId();
        var resp = await RolBLL.ActualizarRol(request);
        return Ok(resp);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarRol(Guid id)
    {
        var resp = await RolBLL.EliminarRol(id, GetOrganizacionId());
        return Ok(resp);
    }
}
