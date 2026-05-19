using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class EntidadBLL
{
    public static async Task<Respuesta<IEnumerable<EntidadResponse>>> ObtenerEntidades(Guid organizacionId)
    {
        var response = new Respuesta<IEnumerable<EntidadResponse>>();
        try
        {
            var datos = await EntidadDAL.ObtenerEntidades(organizacionId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<EntidadResponse?>> ObtenerEntidadPorId(Guid id, Guid organizacionId)
    {
        var response = new Respuesta<EntidadResponse?>();
        try
        {
            var dato = await EntidadDAL.ObtenerEntidadPorId(id, organizacionId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearEntidad(EntidadRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await EntidadDAL.CrearEntidad(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarEntidad(EntidadRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await EntidadDAL.ActualizarEntidad(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarEntidad(Guid id, Guid organizacionId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await EntidadDAL.EliminarEntidad(id, organizacionId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static EntidadResponse Mapear(Entidad e) => new()
    {
        Id             = e.Id,
        OrganizacionId = e.OrganizacionId,
        TipoEntidadId  = e.TipoEntidadId,
        EntidadPadreId = e.EntidadPadreId,
        NombreVisible  = e.NombreVisible,
        IdExterno      = e.IdExterno,
        EsActivo       = e.EsActivo,
        AtributosJson  = e.AtributosJson,
        CreadoEn       = e.CreadoEn
    };
}
