using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class OrganizacionBLL
{
    public static async Task<Respuesta<IEnumerable<OrganizacionResponse>>> ObtenerOrganizaciones()
    {
        var response = new Respuesta<IEnumerable<OrganizacionResponse>>();
        try
        {
            var datos = await OrganizacionDAL.ObtenerOrganizaciones();
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<OrganizacionResponse?>> ObtenerOrganizacionPorId(Guid id)
    {
        var response = new Respuesta<OrganizacionResponse?>();
        try
        {
            var dato = await OrganizacionDAL.ObtenerOrganizacionPorId(id);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearOrganizacion(OrganizacionRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await OrganizacionDAL.CrearOrganizacion(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarOrganizacion(OrganizacionRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await OrganizacionDAL.ActualizarOrganizacion(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarOrganizacion(Guid id)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await OrganizacionDAL.EliminarOrganizacion(id);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static OrganizacionResponse Mapear(Organizacion o) => new()
    {
        Id       = o.Id,
        Nombre   = o.Nombre,
        UrlLogo  = o.UrlLogo,
        CreadoEn = o.CreadoEn
    };
}
