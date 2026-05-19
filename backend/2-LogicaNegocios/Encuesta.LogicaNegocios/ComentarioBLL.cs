using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class ComentarioBLL
{
    public static async Task<Respuesta<IEnumerable<ComentarioResponse>>> ObtenerComentariosPorEntidad(Guid entidadId)
    {
        var response = new Respuesta<IEnumerable<ComentarioResponse>>();
        try
        {
            var datos = await ComentarioDAL.ObtenerComentariosPorEntidad(entidadId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<IEnumerable<ComentarioResponse>>> ObtenerComentariosPorRespuesta(Guid respuestaId)
    {
        var response = new Respuesta<IEnumerable<ComentarioResponse>>();
        try
        {
            var datos = await ComentarioDAL.ObtenerComentariosPorRespuesta(respuestaId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<ComentarioResponse?>> ObtenerComentarioPorId(Guid id)
    {
        var response = new Respuesta<ComentarioResponse?>();
        try
        {
            var dato = await ComentarioDAL.ObtenerComentarioPorId(id);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearComentario(ComentarioRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await ComentarioDAL.CrearComentario(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarComentario(ComentarioRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await ComentarioDAL.ActualizarComentario(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarComentario(Guid id)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await ComentarioDAL.EliminarComentario(id);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static ComentarioResponse Mapear(Comentario c) => new()
    {
        Id               = c.Id,
        EntidadId        = c.EntidadId,
        RespuestaId      = c.RespuestaId,
        UsuarioId        = c.UsuarioId,
        TextoComentario  = c.TextoComentario,
        CreadoEn         = c.CreadoEn
    };
}
