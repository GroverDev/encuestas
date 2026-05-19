using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class EntidadController : ControllerBase
{
    private Guid GetOrganizacionId() =>
        Guid.TryParse(User.Claims.FirstOrDefault(c => c.Type == "OrganizacionId")?.Value, out var id) ? id : Guid.Empty;

    [HttpGet]
    public async Task<ActionResult<Respuesta<IEnumerable<EntidadResponse>>>> ObtenerEntidades()
    {
        var resp = await EntidadBLL.ObtenerEntidades(GetOrganizacionId());
        return Ok(resp);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Respuesta<EntidadResponse?>>> ObtenerEntidadPorId(Guid id)
    {
        var resp = await EntidadBLL.ObtenerEntidadPorId(id, GetOrganizacionId());
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearEntidad([FromBody] EntidadRequest request)
    {
        request.OrganizacionId = GetOrganizacionId();
        var resp = await EntidadBLL.CrearEntidad(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarEntidad([FromBody] EntidadRequest request)
    {
        request.OrganizacionId = GetOrganizacionId();
        var resp = await EntidadBLL.ActualizarEntidad(request);
        return Ok(resp);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarEntidad(Guid id)
    {
        var resp = await EntidadBLL.EliminarEntidad(id, GetOrganizacionId());
        return Ok(resp);
    }
}
