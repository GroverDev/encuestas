using Comun.Herramientas;
using Encuesta.LogicaNegocios;
using Encuesta.ModeloDatos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Encuesta.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("login")]
    public async Task<ActionResult<Respuesta<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        var validacion = await AuthBLL.ValidarCredenciales(request);

        if (!validacion.ok || validacion.Datos is null)
        {
            var error = new Respuesta<LoginResponse>();
            error.SetMensaje(TiposMensaje.Error, "Atención", validacion.Mensaje.Descripcion);
            return Ok(error);
        }

        var usuario = validacion.Datos;
        var token = GenerarToken(usuario, TimeSpan.FromHours(8));

        var resp = new Respuesta<LoginResponse>
        {
            ok = true,
            Datos = new LoginResponse
            {
                Token = token,
                Usuario = new CuentaUsuarioResponse
                {
                    Id               = usuario.Id,
                    OrganizacionId   = usuario.OrganizacionId,
                    EntidadId        = usuario.EntidadId,
                    Correo           = usuario.Correo,
                    EsActivo         = usuario.EsActivo,
                    EsCuentaServicio = usuario.EsCuentaServicio,
                    CreadoEn         = usuario.CreadoEn,
                }
            }
        };
        return Ok(resp);
    }

    [HttpPost("token-servicio")]
    public async Task<ActionResult<Respuesta<LoginResponse>>> TokenServicio([FromBody] LoginRequest request)
    {
        var validacion = await AuthBLL.ValidarCredenciales(request);

        if (!validacion.ok || validacion.Datos is null)
        {
            var error = new Respuesta<LoginResponse>();
            error.SetMensaje(TiposMensaje.Error, "Atención", validacion.Mensaje.Descripcion);
            return Ok(error);
        }

        var usuario = validacion.Datos;

        if (!usuario.EsCuentaServicio)
        {
            var error = new Respuesta<LoginResponse>();
            error.SetMensaje(TiposMensaje.Error, "Acceso denegado", "Esta cuenta no está habilitada para acceso machine-to-machine.");
            return Ok(error);
        }

        var token = GenerarToken(usuario, TimeSpan.FromDays(365));

        var resp = new Respuesta<LoginResponse>
        {
            ok = true,
            Datos = new LoginResponse
            {
                Token = token,
                Usuario = new CuentaUsuarioResponse
                {
                    Id               = usuario.Id,
                    OrganizacionId   = usuario.OrganizacionId,
                    EntidadId        = usuario.EntidadId,
                    Correo           = usuario.Correo,
                    EsActivo         = usuario.EsActivo,
                    EsCuentaServicio = usuario.EsCuentaServicio,
                    CreadoEn         = usuario.CreadoEn,
                }
            }
        };
        return Ok(resp);
    }

    private string GenerarToken(CuentaUsuario usuario, TimeSpan duracion)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim("UsuarioId",        usuario.Id.ToString()),
            new Claim("OrganizacionId",   usuario.OrganizacionId.ToString()),
            new Claim("EsCuentaServicio", usuario.EsCuentaServicio.ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:             _config["Jwt:Issuer"],
            audience:           _config["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.Add(duracion),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
