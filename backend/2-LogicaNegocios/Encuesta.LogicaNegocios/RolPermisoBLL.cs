using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class RolPermisoBLL
{
    public static async Task<Respuesta<IEnumerable<RolPermisoResponse>>> ObtenerRolPermisos(Guid rolId)
    {
        var response = new Respuesta<IEnumerable<RolPermisoResponse>>();
        try
        {
            var datos = await RolPermisoDAL.ObtenerRolPermisos(rolId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> AsignarPermisoRol(RolPermisoRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await RolPermisoDAL.AsignarPermisoRol(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> RemoverPermisoRol(Guid rolId, Guid permisoId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await RolPermisoDAL.RemoverPermisoRol(rolId, permisoId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static RolPermisoResponse Mapear(RolPermiso rp) => new()
    {
        RolId     = rp.RolId,
        PermisoId = rp.PermisoId
    };
}
