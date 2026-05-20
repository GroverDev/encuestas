using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private Guid GetOrganizacionId() =>
        Guid.TryParse(User.Claims.FirstOrDefault(c => c.Type == "OrganizacionId")?.Value, out var id) ? id : Guid.Empty;

    [HttpGet("stats")]
    public async Task<ActionResult<Respuesta<DashboardStatsResponse>>> ObtenerStats()
    {
        var resp = await DashboardBLL.ObtenerStats(GetOrganizacionId());
        return Ok(resp);
    }
}
