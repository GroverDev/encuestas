using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class PreguntaBLL
{
    public static async Task<Respuesta<IEnumerable<PreguntaResponse>>> ObtenerPreguntas(Guid encuestaId)
    {
        var response = new Respuesta<IEnumerable<PreguntaResponse>>();
        try
        {
            var datos = await PreguntaDAL.ObtenerPreguntas(encuestaId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<PreguntaResponse?>> ObtenerPreguntaPorId(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<PreguntaResponse?>();
        try
        {
            var dato = await PreguntaDAL.ObtenerPreguntaPorId(id, encuestaId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearPregunta(PreguntaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await PreguntaDAL.CrearPregunta(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarPregunta(PreguntaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await PreguntaDAL.ActualizarPregunta(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarPregunta(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await PreguntaDAL.EliminarPregunta(id, encuestaId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static PreguntaResponse Mapear(Pregunta p) => new()
    {
        Id                = p.Id,
        EncuestaId        = p.EncuestaId,
        SeccionId         = p.SeccionId,
        DimensionId       = p.DimensionId,
        Tipo              = p.Tipo,
        Titulo            = p.Titulo,
        Descripcion       = p.Descripcion,
        Orden             = p.Orden,
        Peso              = p.Peso,
        EsObligatoria     = p.EsObligatoria,
        ConfiguracionJson = p.ConfiguracionJson,
        CreadoEn          = p.CreadoEn
    };
}
