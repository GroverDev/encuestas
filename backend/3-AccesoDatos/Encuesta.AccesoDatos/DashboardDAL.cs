using Comun.BaseDatos;
using Comun.Herramientas;
using Dapper;
using Npgsql;
using System.Net.Sockets;
using DB = Comun.BaseDatos.Enumeradores;
using Encuesta.ModeloDatos;

namespace Encuesta.AccesoDatos;

public static class DashboardDAL
{
    public static async Task<DashboardStatsResponse> ObtenerStats(Guid organizacionId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            const string sql = """
                SELECT
                    (SELECT COUNT(*) FROM encuesta
                     WHERE organizacion_id = @OrgId)::int                                                AS total_encuestas,
                    (SELECT COUNT(*) FROM encuesta
                     WHERE organizacion_id = @OrgId AND estado = 'PUBLICADA')::int                       AS encuestas_activas,
                    (SELECT COUNT(*) FROM respuesta r
                     JOIN encuesta e ON e.id = r.encuesta_id
                     WHERE e.organizacion_id = @OrgId)::int                                              AS total_respuestas,
                    (SELECT COUNT(*) FROM invitacion i
                     JOIN encuesta e ON e.id = i.encuesta_id
                     WHERE e.organizacion_id = @OrgId)::int                                              AS total_invitaciones
                """;
            return await db.CreateConnection.QuerySingleAsync<DashboardStatsResponse>(sql, new { OrgId = organizacionId });
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }
}
