using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class SeccionEncuestaBLL
{
    public static async Task<Respuesta<IEnumerable<SeccionEncuestaResponse>>> ObtenerSecciones(Guid encuestaId)
    {
        var response = new Respuesta<IEnumerable<SeccionEncuestaResponse>>();
        try
        {
            var datos = await SeccionEncuestaDAL.ObtenerSecciones(encuestaId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<SeccionEncuestaResponse?>> ObtenerSeccionPorId(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<SeccionEncuestaResponse?>();
        try
        {
            var dato = await SeccionEncuestaDAL.ObtenerSeccionPorId(id, encuestaId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearSeccion(SeccionEncuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await SeccionEncuestaDAL.CrearSeccion(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarSeccion(SeccionEncuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await SeccionEncuestaDAL.ActualizarSeccion(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarSeccion(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await SeccionEncuestaDAL.EliminarSeccion(id, encuestaId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static SeccionEncuestaResponse Mapear(SeccionEncuesta s) => new()
    {
        Id          = s.Id,
        EncuestaId  = s.EncuestaId,
        Titulo      = s.Titulo,
        Descripcion = s.Descripcion,
        Orden       = s.Orden
    };
}
