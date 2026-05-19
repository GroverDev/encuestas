using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class RespuestaBLL
{
    public static async Task<Respuesta<IEnumerable<RespuestaResponse>>> ObtenerRespuestas(Guid encuestaId)
    {
        var response = new Respuesta<IEnumerable<RespuestaResponse>>();
        try
        {
            var datos = await RespuestaDAL.ObtenerRespuestas(encuestaId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<RespuestaResponse?>> ObtenerRespuestaPorId(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<RespuestaResponse?>();
        try
        {
            var dato = await RespuestaDAL.ObtenerRespuestaPorId(id, encuestaId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearRespuesta(RespuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await RespuestaDAL.CrearRespuesta(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarRespuesta(RespuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await RespuestaDAL.ActualizarRespuesta(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarRespuesta(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await RespuestaDAL.EliminarRespuesta(id, encuestaId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static RespuestaResponse Mapear(Respuesta r) => new()
    {
        Id                     = r.Id,
        EncuestaId             = r.EncuestaId,
        VersionEncuesta        = r.VersionEncuesta,
        InvitacionId           = r.InvitacionId,
        UsuarioRespondentId    = r.UsuarioRespondentId,
        Canal                  = r.Canal,
        UltimaPreguntaId       = r.UltimaPreguntaId,
        PesoEstadistico        = r.PesoEstadistico,
        ConsentimientoOtorgado = r.ConsentimientoOtorgado,
        FechaConsentimiento    = r.FechaConsentimiento,
        IniciadoEn             = r.IniciadoEn,
        CompletadoEn           = r.CompletadoEn,
        InfoDispositivo        = r.InfoDispositivo,
        DireccionIp            = r.DireccionIp
    };
}
