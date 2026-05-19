using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class NotificacionEnvioBLL
{
    public static async Task<Respuesta<IEnumerable<NotificacionEnvioResponse>>> ObtenerNotificaciones(Guid invitacionId)
    {
        var response = new Respuesta<IEnumerable<NotificacionEnvioResponse>>();
        try
        {
            var datos = await NotificacionEnvioDAL.ObtenerNotificaciones(invitacionId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<NotificacionEnvioResponse?>> ObtenerNotificacionPorId(Guid id, Guid invitacionId)
    {
        var response = new Respuesta<NotificacionEnvioResponse?>();
        try
        {
            var dato = await NotificacionEnvioDAL.ObtenerNotificacionPorId(id, invitacionId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> RegistrarNotificacion(NotificacionEnvioRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await NotificacionEnvioDAL.RegistrarNotificacion(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static NotificacionEnvioResponse Mapear(NotificacionEnvio n) => new()
    {
        Id            = n.Id,
        InvitacionId  = n.InvitacionId,
        Tipo          = n.Tipo,
        Canal         = n.Canal,
        Destinatario  = n.Destinatario,
        Estado        = n.Estado,
        IntentosEnvio = n.IntentosEnvio,
        EnviadoEn     = n.EnviadoEn,
        EntregadoEn   = n.EntregadoEn,
        ErrorDetalle  = n.ErrorDetalle,
        CreadoEn      = n.CreadoEn
    };
}
