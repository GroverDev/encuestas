using Comun.Herramientas;
using Comun.Herramientas.Criptografia;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Encuesta.Api.Controllers;

/// <summary>
/// TEMPORAL — eliminar después de la configuración inicial.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class SeedController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> CrearDatosIniciales([FromBody] SeedRequest req)
    {
        try
        {
            // 1. Organización
            var orgRequest = new OrganizacionRequest { Nombre = req.NombreOrganizacion };
            await OrganizacionDAL.CrearOrganizacion(orgRequest);

            var orgs = await OrganizacionDAL.ObtenerOrganizaciones();
            var org = orgs.FirstOrDefault(o => o.Nombre == req.NombreOrganizacion);
            if (org is null) return BadRequest("No se pudo crear la organización.");

            // 2. Usuario administrador
            var usuarioRequest = new CuentaUsuarioRequest
            {
                OrganizacionId  = org.Id,
                Correo          = req.Correo,
                HashContrasena  = ASimetrica.HashPassword(req.Contrasena),
            };
            await CuentaUsuarioDAL.CrearCuentaUsuario(usuarioRequest);

            return Ok(new
            {
                ok = true,
                mensaje = "Datos iniciales creados correctamente.",
                organizacionId = org.Id,
                correo = req.Correo,
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { ok = false, mensaje = ex.Message });
        }
    }

    public class SeedRequest
    {
        public string NombreOrganizacion { get; set; } = "";
        public string Correo { get; set; } = "";
        public string Contrasena { get; set; } = "";
    }
}
