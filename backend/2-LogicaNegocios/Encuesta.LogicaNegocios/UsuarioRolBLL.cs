using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class UsuarioRolBLL
{
    public static async Task<Respuesta<IEnumerable<UsuarioRolResponse>>> ObtenerUsuarioRoles(Guid usuarioId)
    {
        var response = new Respuesta<IEnumerable<UsuarioRolResponse>>();
        try
        {
            var datos = await UsuarioRolDAL.ObtenerUsuarioRoles(usuarioId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> AsignarRolUsuario(UsuarioRolRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await UsuarioRolDAL.AsignarRolUsuario(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> RemoverRolUsuario(Guid usuarioId, Guid rolId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await UsuarioRolDAL.RemoverRolUsuario(usuarioId, rolId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static UsuarioRolResponse Mapear(UsuarioRol ur) => new()
    {
        UsuarioId = ur.UsuarioId,
        RolId     = ur.RolId
    };
}
