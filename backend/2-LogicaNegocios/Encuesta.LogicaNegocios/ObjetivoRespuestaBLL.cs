using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class ObjetivoRespuestaBLL
{
    public static async Task<Respuesta<IEnumerable<ObjetivoRespuestaResponse>>> ObtenerObjetivos(Guid respuestaId)
    {
        var response = new Respuesta<IEnumerable<ObjetivoRespuestaResponse>>();
        try
        {
            var datos = await ObjetivoRespuestaDAL.ObtenerObjetivos(respuestaId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<ObjetivoRespuestaResponse?>> ObtenerObjetivoPorId(Guid id, Guid respuestaId)
    {
        var response = new Respuesta<ObjetivoRespuestaResponse?>();
        try
        {
            var dato = await ObjetivoRespuestaDAL.ObtenerObjetivoPorId(id, respuestaId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearObjetivo(ObjetivoRespuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await ObjetivoRespuestaDAL.CrearObjetivo(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarObjetivo(ObjetivoRespuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await ObjetivoRespuestaDAL.ActualizarObjetivo(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarObjetivo(Guid id, Guid respuestaId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await ObjetivoRespuestaDAL.EliminarObjetivo(id, respuestaId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static ObjetivoRespuestaResponse Mapear(ObjetivoRespuesta o) => new()
    {
        Id           = o.Id,
        RespuestaId  = o.RespuestaId,
        EntidadId    = o.EntidadId,
        TipoRelacion = o.TipoRelacion
    };
}
