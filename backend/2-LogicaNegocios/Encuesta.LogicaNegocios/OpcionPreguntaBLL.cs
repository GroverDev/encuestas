using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class OpcionPreguntaBLL
{
    public static async Task<Respuesta<IEnumerable<OpcionPreguntaResponse>>> ObtenerOpciones(Guid preguntaId)
    {
        var response = new Respuesta<IEnumerable<OpcionPreguntaResponse>>();
        try
        {
            var datos = await OpcionPreguntaDAL.ObtenerOpciones(preguntaId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<OpcionPreguntaResponse?>> ObtenerOpcionPorId(Guid id, Guid preguntaId)
    {
        var response = new Respuesta<OpcionPreguntaResponse?>();
        try
        {
            var dato = await OpcionPreguntaDAL.ObtenerOpcionPorId(id, preguntaId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearOpcion(OpcionPreguntaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await OpcionPreguntaDAL.CrearOpcion(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarOpcion(OpcionPreguntaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await OpcionPreguntaDAL.ActualizarOpcion(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarOpcion(Guid id, Guid preguntaId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await OpcionPreguntaDAL.EliminarOpcion(id, preguntaId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static OpcionPreguntaResponse Mapear(OpcionPregunta o) => new()
    {
        Id         = o.Id,
        PreguntaId = o.PreguntaId,
        Etiqueta   = o.Etiqueta,
        Valor      = o.Valor,
        Puntaje    = o.Puntaje,
        Orden      = o.Orden
    };
}
