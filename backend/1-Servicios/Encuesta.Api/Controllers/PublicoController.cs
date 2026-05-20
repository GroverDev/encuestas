using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class PublicoController : ControllerBase
{
    [HttpGet("encuesta/{token:guid}")]
    public async Task<ActionResult<Respuesta<EncuestaPublicaResponse?>>> ObtenerEncuestaPublica(Guid token)
    {
        var resp = await PublicoBLL.ObtenerEncuestaPublica(token);
        return Ok(resp);
    }

    [HttpPost("responder/{token:guid}")]
    public async Task<ActionResult<Respuesta<bool>>> GuardarRespuesta(
        Guid token, [FromBody] SubmitRespuestaPublicaRequest request)
    {
        var resp = await PublicoBLL.GuardarRespuesta(token, request);
        return Ok(resp);
    }
}
