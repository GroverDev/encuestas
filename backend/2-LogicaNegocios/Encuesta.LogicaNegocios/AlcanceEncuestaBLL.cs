using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class AlcanceEncuestaBLL
{
    public static async Task<Respuesta<IEnumerable<AlcanceEncuestaResponse>>> ObtenerAlcances(Guid encuestaId)
    {
        var response = new Respuesta<IEnumerable<AlcanceEncuestaResponse>>();
        try
        {
            var datos = await AlcanceEncuestaDAL.ObtenerAlcances(encuestaId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<AlcanceEncuestaResponse?>> ObtenerAlcancePorId(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<AlcanceEncuestaResponse?>();
        try
        {
            var dato = await AlcanceEncuestaDAL.ObtenerAlcancePorId(id, encuestaId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearAlcance(AlcanceEncuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await AlcanceEncuestaDAL.CrearAlcance(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarAlcance(AlcanceEncuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await AlcanceEncuestaDAL.ActualizarAlcance(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarAlcance(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await AlcanceEncuestaDAL.EliminarAlcance(id, encuestaId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static AlcanceEncuestaResponse Mapear(AlcanceEncuesta a) => new()
    {
        Id                   = a.Id,
        EncuestaId           = a.EncuestaId,
        EntidadId            = a.EntidadId,
        TipoRelacion         = a.TipoRelacion,
        IncluirDescendientes = a.IncluirDescendientes
    };
}
