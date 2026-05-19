using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class InvitacionBLL
{
    public static async Task<Respuesta<IEnumerable<InvitacionResponse>>> ObtenerInvitaciones(Guid encuestaId)
    {
        var response = new Respuesta<IEnumerable<InvitacionResponse>>();
        try
        {
            var datos = await InvitacionDAL.ObtenerInvitaciones(encuestaId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<InvitacionResponse?>> ObtenerInvitacionPorId(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<InvitacionResponse?>();
        try
        {
            var dato = await InvitacionDAL.ObtenerInvitacionPorId(id, encuestaId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearInvitacion(InvitacionRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await InvitacionDAL.CrearInvitacion(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarInvitacion(InvitacionRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await InvitacionDAL.ActualizarInvitacion(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarInvitacion(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await InvitacionDAL.EliminarInvitacion(id, encuestaId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static InvitacionResponse Mapear(Invitacion i) => new()
    {
        Id                = i.Id,
        EncuestaId        = i.EncuestaId,
        CuentaUsuarioId   = i.CuentaUsuarioId,
        CorreoDestino     = i.CorreoDestino,
        EntidadEvaluadaId = i.EntidadEvaluadaId,
        TokenAcceso       = i.TokenAcceso,
        Canal             = i.Canal,
        Estado            = i.Estado,
        EnviadoEn         = i.EnviadoEn,
        VenceEn           = i.VenceEn,
        RespondidoEn      = i.RespondidoEn
    };
}
