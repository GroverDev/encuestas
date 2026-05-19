using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrganizacionController : ControllerBase
{
    private Guid GetOrganizacionId() =>
        Guid.TryParse(User.Claims.FirstOrDefault(c => c.Type == "OrganizacionId")?.Value, out var id) ? id : Guid.Empty;

    [HttpGet]
    public async Task<ActionResult<Respuesta<IEnumerable<OrganizacionResponse>>>> ObtenerOrganizaciones()
    {
        var resp = await OrganizacionBLL.ObtenerOrganizaciones();
        return Ok(resp);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Respuesta<OrganizacionResponse?>>> ObtenerOrganizacionPorId(Guid id)
    {
        var resp = await OrganizacionBLL.ObtenerOrganizacionPorId(id);
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearOrganizacion([FromBody] OrganizacionRequest request)
    {
        var resp = await OrganizacionBLL.CrearOrganizacion(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarOrganizacion([FromBody] OrganizacionRequest request)
    {
        var resp = await OrganizacionBLL.ActualizarOrganizacion(request);
        return Ok(resp);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarOrganizacion(Guid id)
    {
        var resp = await OrganizacionBLL.EliminarOrganizacion(id);
        return Ok(resp);
    }
}
