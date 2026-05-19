using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class AdjuntoBLL
{
    public static async Task<Respuesta<IEnumerable<AdjuntoResponse>>> ObtenerAdjuntos(Guid entidadId)
    {
        var response = new Respuesta<IEnumerable<AdjuntoResponse>>();
        try
        {
            var datos = await AdjuntoDAL.ObtenerAdjuntos(entidadId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<AdjuntoResponse?>> ObtenerAdjuntoPorId(Guid id, Guid entidadId)
    {
        var response = new Respuesta<AdjuntoResponse?>();
        try
        {
            var dato = await AdjuntoDAL.ObtenerAdjuntoPorId(id, entidadId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearAdjunto(AdjuntoRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await AdjuntoDAL.CrearAdjunto(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarAdjunto(AdjuntoRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await AdjuntoDAL.ActualizarAdjunto(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarAdjunto(Guid id, Guid entidadId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await AdjuntoDAL.EliminarAdjunto(id, entidadId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static AdjuntoResponse Mapear(Adjunto a) => new()
    {
        Id            = a.Id,
        EntidadId     = a.EntidadId,
        NombreArchivo = a.NombreArchivo,
        UrlArchivo    = a.UrlArchivo,
        SubidoEn      = a.SubidoEn
    };
}
