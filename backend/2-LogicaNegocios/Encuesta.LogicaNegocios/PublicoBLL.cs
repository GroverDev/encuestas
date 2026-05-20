using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class PublicoBLL
{
    public static async Task<Respuesta<EncuestaPublicaResponse?>> ObtenerEncuestaPublica(Guid token)
    {
        var response = new Respuesta<EncuestaPublicaResponse?>();
        try
        {
            var datos = await PublicoDAL.ObtenerEncuestaPublica(token);
            if (datos is null)
                throw new ExceptionControlado("La encuesta no está disponible o el enlace no es válido.");
            response.Datos = datos;
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<bool>> GuardarRespuesta(Guid token, SubmitRespuestaPublicaRequest request)
    {
        var response = new Respuesta<bool>();
        try
        {
            response.Datos = await PublicoDAL.GuardarRespuesta(token, request);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }
}
