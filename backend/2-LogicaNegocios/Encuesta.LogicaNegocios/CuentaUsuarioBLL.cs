using Comun.Herramientas;
using Comun.Herramientas.Criptografia;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class CuentaUsuarioBLL
{
    public static async Task<Respuesta<IEnumerable<CuentaUsuarioResponse>>> ObtenerCuentasUsuario(Guid organizacionId)
    {
        var response = new Respuesta<IEnumerable<CuentaUsuarioResponse>>();
        try
        {
            var datos = await CuentaUsuarioDAL.ObtenerCuentasUsuario(organizacionId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<CuentaUsuarioResponse?>> ObtenerCuentaUsuarioPorId(Guid id, Guid organizacionId)
    {
        var response = new Respuesta<CuentaUsuarioResponse?>();
        try
        {
            var dato = await CuentaUsuarioDAL.ObtenerCuentaUsuarioPorId(id, organizacionId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearCuentaUsuario(CuentaUsuarioRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            request.HashContrasena = ASimetrica.HashPassword(request.Contrasena);
            response.Datos = await CuentaUsuarioDAL.CrearCuentaUsuario(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarCuentaUsuario(CuentaUsuarioRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            request.HashContrasena = ASimetrica.HashPassword(request.Contrasena);
            response.Datos = await CuentaUsuarioDAL.ActualizarCuentaUsuario(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarCuentaUsuario(Guid id, Guid organizacionId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await CuentaUsuarioDAL.EliminarCuentaUsuario(id, organizacionId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static CuentaUsuarioResponse Mapear(CuentaUsuario c) => new()
    {
        Id             = c.Id,
        OrganizacionId = c.OrganizacionId,
        EntidadId      = c.EntidadId,
        Correo         = c.Correo,
        EsActivo       = c.EsActivo,
        CreadoEn       = c.CreadoEn
    };
}
