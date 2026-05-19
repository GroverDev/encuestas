using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class CuotaRespuestaBLL
{
    public static async Task<Respuesta<IEnumerable<CuotaRespuestaResponse>>> ObtenerCuotas(Guid encuestaId)
    {
        var response = new Respuesta<IEnumerable<CuotaRespuestaResponse>>();
        try
        {
            var datos = await CuotaRespuestaDAL.ObtenerCuotas(encuestaId);
            response.Datos = datos.Select(Mapear);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<CuotaRespuestaResponse?>> ObtenerCuotaPorId(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<CuotaRespuestaResponse?>();
        try
        {
            var dato = await CuotaRespuestaDAL.ObtenerCuotaPorId(id, encuestaId);
            response.Datos = dato is null ? null : Mapear(dato);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> CrearCuota(CuotaRespuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await CuotaRespuestaDAL.CrearCuota(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> ActualizarCuota(CuotaRespuestaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await CuotaRespuestaDAL.ActualizarCuota(request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> EliminarCuota(Guid id, Guid encuestaId)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await CuotaRespuestaDAL.EliminarCuota(id, encuestaId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static CuotaRespuestaResponse Mapear(CuotaRespuesta c) => new()
    {
        Id               = c.Id,
        EncuestaId       = c.EncuestaId,
        EntidadId        = c.EntidadId,
        Limite           = c.Limite,
        TotalActual      = c.TotalActual,
        CerrarAlAlcanzar = c.CerrarAlAlcanzar
    };
}
