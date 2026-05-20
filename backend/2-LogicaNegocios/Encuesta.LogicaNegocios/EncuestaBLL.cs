using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class EncuestaBLL
{
    public static async Task<Respuesta<IEnumerable<EncuestaResponse>>> ObtenerEncuestas(Guid organizacionId)
    {
        var response = new Respuesta<IEnumerable<EncuestaResponse>>();
        try
        {
            var datos = await EncuestaDAL.ObtenerEncuestas(organizacionId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<EncuestaResponse?>> ObtenerEncuestaPorId(Guid id, Guid organizacionId)
    {
        var response = new Respuesta<EncuestaResponse?>();
        try
        {
            var dato = await EncuestaDAL.ObtenerEncuestaPorId(id, organizacionId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearEncuesta(EncuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await EncuestaDAL.CrearEncuesta(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarEncuesta(EncuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await EncuestaDAL.ActualizarEncuesta(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CambiarEstado(Guid id, Guid organizacionId, string nuevoEstado)
    {
        var response = new Respuesta<bool>();
        try
        {
            var estadosValidos = new[] { "PUBLICADA", "CERRADA" };
            if (!estadosValidos.Contains(nuevoEstado))
                throw new ExceptionControlado($"Estado '{nuevoEstado}' no válido.");

            var actualizo = await EncuestaDAL.CambiarEstadoEncuesta(id, organizacionId, nuevoEstado);
            if (!actualizo)
                throw new ExceptionControlado("No se pudo cambiar el estado. Verifica que la encuesta existe y que la transición es válida.");

            response.Datos = true;
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarEncuesta(Guid id, Guid organizacionId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await EncuestaDAL.EliminarEncuesta(id, organizacionId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static EncuestaResponse Mapear(ModeloDatos.Encuesta e) => new()
    {
        Id                 = e.Id,
        OrganizacionId     = e.OrganizacionId,
        Titulo             = e.Titulo,
        Descripcion        = e.Descripcion,
        Version            = e.Version,
        Estado             = e.Estado,
        EsGlobal           = e.EsGlobal,
        EsPlantilla        = e.EsPlantilla,
        PlantillaOrigenId  = e.PlantillaOrigenId,
        EtiquetasJson      = e.EtiquetasJson,
        CreadoPorUsuarioId = e.CreadoPorUsuarioId,
        FechaInicio        = e.FechaInicio,
        FechaFin           = e.FechaFin,
        PublicadoEn        = e.PublicadoEn,
        ConfiguracionJson  = e.ConfiguracionJson,
        CreadoEn           = e.CreadoEn
    };
}
