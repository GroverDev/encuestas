using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class ReglaEncuestaBLL
{
    public static async Task<Respuesta<IEnumerable<ReglaEncuestaResponse>>> ObtenerReglasEncuesta(Guid encuestaId)
    {
        var response = new Respuesta<IEnumerable<ReglaEncuestaResponse>>();
        try
        {
            var datos = await ReglaEncuestaDAL.ObtenerReglasEncuesta(encuestaId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<ReglaEncuestaResponse?>> ObtenerReglaEncuestaPorId(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<ReglaEncuestaResponse?>();
        try
        {
            var dato = await ReglaEncuestaDAL.ObtenerReglaEncuestaPorId(id, encuestaId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearReglaEncuesta(ReglaEncuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await ReglaEncuestaDAL.CrearReglaEncuesta(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarReglaEncuesta(ReglaEncuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await ReglaEncuestaDAL.ActualizarReglaEncuesta(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarReglaEncuesta(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await ReglaEncuestaDAL.EliminarReglaEncuesta(id, encuestaId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static ReglaEncuestaResponse Mapear(ReglaEncuesta r) => new()
    {
        Id         = r.Id,
        EncuestaId = r.EncuestaId,
        ReglaJson  = r.ReglaJson
    };
}
