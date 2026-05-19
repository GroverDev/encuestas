using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class DetalleRespuestaBLL
{
    public static async Task<Respuesta<IEnumerable<DetalleRespuestaResponse>>> ObtenerDetallesRespuesta(Guid respuestaId)
    {
        var response = new Respuesta<IEnumerable<DetalleRespuestaResponse>>();
        try
        {
            var datos = await DetalleRespuestaDAL.ObtenerDetallesRespuesta(respuestaId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<DetalleRespuestaResponse?>> ObtenerDetalleRespuestaPorId(Guid id, Guid respuestaId)
    {
        var response = new Respuesta<DetalleRespuestaResponse?>();
        try
        {
            var dato = await DetalleRespuestaDAL.ObtenerDetalleRespuestaPorId(id, respuestaId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearDetalleRespuesta(DetalleRespuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await DetalleRespuestaDAL.CrearDetalleRespuesta(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarDetalleRespuesta(DetalleRespuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await DetalleRespuestaDAL.ActualizarDetalleRespuesta(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarDetalleRespuesta(Guid id, Guid respuestaId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await DetalleRespuestaDAL.EliminarDetalleRespuesta(id, respuestaId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static DetalleRespuestaResponse Mapear(DetalleRespuesta d) => new()
    {
        Id            = d.Id,
        RespuestaId   = d.RespuestaId,
        PreguntaId    = d.PreguntaId,
        ValorTexto    = d.ValorTexto,
        ValorNumero   = d.ValorNumero,
        ValorBooleano = d.ValorBooleano,
        ValorFecha    = d.ValorFecha,
        ValorJson     = d.ValorJson
    };
}
