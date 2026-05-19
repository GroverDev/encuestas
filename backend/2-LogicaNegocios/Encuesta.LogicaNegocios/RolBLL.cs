using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class RolBLL
{
    public static async Task<Respuesta<IEnumerable<RolResponse>>> ObtenerRoles(Guid organizacionId)
    {
        var response = new Respuesta<IEnumerable<RolResponse>>();
        try
        {
            var datos = await RolDAL.ObtenerRoles(organizacionId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<RolResponse?>> ObtenerRolPorId(Guid id, Guid organizacionId)
    {
        var response = new Respuesta<RolResponse?>();
        try
        {
            var dato = await RolDAL.ObtenerRolPorId(id, organizacionId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearRol(RolRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await RolDAL.CrearRol(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarRol(RolRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await RolDAL.ActualizarRol(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarRol(Guid id, Guid organizacionId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await RolDAL.EliminarRol(id, organizacionId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static RolResponse Mapear(Rol r) => new()
    {
        Id             = r.Id,
        OrganizacionId = r.OrganizacionId,
        Nombre         = r.Nombre
    };
}
