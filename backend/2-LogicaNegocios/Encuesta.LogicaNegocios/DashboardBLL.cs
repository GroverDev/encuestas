using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class DashboardBLL
{
    public static async Task<Respuesta<DashboardStatsResponse>> ObtenerStats(Guid organizacionId)
    {
        var response = new Respuesta<DashboardStatsResponse>();
        try
        {
            response.Datos = await DashboardDAL.ObtenerStats(organizacionId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }
}
