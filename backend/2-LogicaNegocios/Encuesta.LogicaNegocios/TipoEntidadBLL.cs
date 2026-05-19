using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class TipoEntidadBLL
{
    public static async Task<Respuesta<IEnumerable<TipoEntidadResponse>>> ObtenerTiposEntidad()
    {
        var response = new Respuesta<IEnumerable<TipoEntidadResponse>>();
        try
        {
            var datos = await TipoEntidadDAL.ObtenerTiposEntidad();
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<TipoEntidadResponse?>> ObtenerTipoEntidadPorId(Guid id)
    {
        var response = new Respuesta<TipoEntidadResponse?>();
        try
        {
            var dato = await TipoEntidadDAL.ObtenerTipoEntidadPorId(id);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearTipoEntidad(TipoEntidadRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await TipoEntidadDAL.CrearTipoEntidad(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarTipoEntidad(TipoEntidadRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await TipoEntidadDAL.ActualizarTipoEntidad(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarTipoEntidad(Guid id)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await TipoEntidadDAL.EliminarTipoEntidad(id);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static TipoEntidadResponse Mapear(TipoEntidad t) => new()
    {
        Id     = t.Id,
        Codigo = t.Codigo,
        Nombre = t.Nombre
    };
}
