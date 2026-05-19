using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class ColaboradorEncuestaBLL
{
    public static async Task<Respuesta<IEnumerable<ColaboradorEncuestaResponse>>> ObtenerColaboradores(Guid encuestaId)
    {
        var response = new Respuesta<IEnumerable<ColaboradorEncuestaResponse>>();
        try
        {
            var datos = await ColaboradorEncuestaDAL.ObtenerColaboradores(encuestaId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<ColaboradorEncuestaResponse?>> ObtenerColaboradorPorId(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<ColaboradorEncuestaResponse?>();
        try
        {
            var dato = await ColaboradorEncuestaDAL.ObtenerColaboradorPorId(id, encuestaId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearColaborador(ColaboradorEncuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await ColaboradorEncuestaDAL.CrearColaborador(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarColaborador(ColaboradorEncuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await ColaboradorEncuestaDAL.ActualizarColaborador(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarColaborador(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await ColaboradorEncuestaDAL.EliminarColaborador(id, encuestaId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static ColaboradorEncuestaResponse Mapear(ColaboradorEncuesta c) => new()
    {
        Id         = c.Id,
        EncuestaId = c.EncuestaId,
        UsuarioId  = c.UsuarioId,
        Rol        = c.Rol,
        InvitadoEn = c.InvitadoEn
    };
}
