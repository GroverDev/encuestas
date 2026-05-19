using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class PermisoBLL
{
    public static async Task<Respuesta<IEnumerable<PermisoResponse>>> ObtenerPermisos()
    {
        var response = new Respuesta<IEnumerable<PermisoResponse>>();
        try
        {
            var datos = await PermisoDAL.ObtenerPermisos();
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<PermisoResponse?>> ObtenerPermisoPorId(Guid id)
    {
        var response = new Respuesta<PermisoResponse?>();
        try
        {
            var dato = await PermisoDAL.ObtenerPermisoPorId(id);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearPermiso(PermisoRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await PermisoDAL.CrearPermiso(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarPermiso(PermisoRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await PermisoDAL.ActualizarPermiso(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarPermiso(Guid id)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await PermisoDAL.EliminarPermiso(id);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static PermisoResponse Mapear(Permiso p) => new()
    {
        Id     = p.Id,
        Codigo = p.Codigo,
        Nombre = p.Nombre
    };
}
