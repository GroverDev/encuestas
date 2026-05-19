using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CuentaUsuarioController : ControllerBase
{
    private Guid GetOrganizacionId() =>
        Guid.TryParse(User.Claims.FirstOrDefault(c => c.Type == "OrganizacionId")?.Value, out var id) ? id : Guid.Empty;

    [HttpGet]
    public async Task<ActionResult<Respuesta<IEnumerable<CuentaUsuarioResponse>>>> ObtenerCuentasUsuario()
    {
        var resp = await CuentaUsuarioBLL.ObtenerCuentasUsuario(GetOrganizacionId());
        return Ok(resp);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Respuesta<CuentaUsuarioResponse?>>> ObtenerCuentaUsuarioPorId(Guid id)
    {
        var resp = await CuentaUsuarioBLL.ObtenerCuentaUsuarioPorId(id, GetOrganizacionId());
        return Ok(resp);
    }

    [HttpPost]
    public async Task<ActionResult<Respuesta<bool>>> CrearCuentaUsuario([FromBody] CuentaUsuarioRequest request)
    {
        request.OrganizacionId = GetOrganizacionId();
        var resp = await CuentaUsuarioBLL.CrearCuentaUsuario(request);
        return Ok(resp);
    }

    [HttpPut]
    public async Task<ActionResult<Respuesta<bool>>> ActualizarCuentaUsuario([FromBody] CuentaUsuarioRequest request)
    {
        request.OrganizacionId = GetOrganizacionId();
        var resp = await CuentaUsuarioBLL.ActualizarCuentaUsuario(request);
        return Ok(resp);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Respuesta<bool>>> EliminarCuentaUsuario(Guid id)
    {
        var resp = await CuentaUsuarioBLL.EliminarCuentaUsuario(id, GetOrganizacionId());
        return Ok(resp);
    }
}
