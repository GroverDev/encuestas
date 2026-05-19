using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class DimensionPreguntaBLL
{
    public static async Task<Respuesta<IEnumerable<DimensionPreguntaResponse>>> ObtenerDimensiones(Guid encuestaId)
    {
        var response = new Respuesta<IEnumerable<DimensionPreguntaResponse>>();
        try
        {
            var datos = await DimensionPreguntaDAL.ObtenerDimensiones(encuestaId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<DimensionPreguntaResponse?>> ObtenerDimensionPorId(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<DimensionPreguntaResponse?>();
        try
        {
            var dato = await DimensionPreguntaDAL.ObtenerDimensionPorId(id, encuestaId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearDimension(DimensionPreguntaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await DimensionPreguntaDAL.CrearDimension(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarDimension(DimensionPreguntaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await DimensionPreguntaDAL.ActualizarDimension(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarDimension(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await DimensionPreguntaDAL.EliminarDimension(id, encuestaId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static DimensionPreguntaResponse Mapear(DimensionPregunta d) => new()
    {
        Id          = d.Id,
        EncuestaId  = d.EncuestaId,
        Nombre      = d.Nombre,
        Descripcion = d.Descripcion,
        Peso        = d.Peso,
        Orden       = d.Orden
    };
}
