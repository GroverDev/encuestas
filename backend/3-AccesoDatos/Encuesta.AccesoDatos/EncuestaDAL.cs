using Comun.BaseDatos;
using Comun.Herramientas;
using Dapper;
using Npgsql;
using System.Net.Sockets;
using DB = Comun.BaseDatos.Enumeradores;
using EncuestaEntity = Encuesta.ModeloDatos.Encuesta;
using Encuesta.ModeloDatos;

namespace Encuesta.AccesoDatos;

public static class EncuestaDAL
{
    public static async Task<IEnumerable<EncuestaEntity>> ObtenerEncuestas(Guid organizacionId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            const string sql = """
                SELECT id, organizacionid, titulo, descripcion, version, estado,
                       esglobal, esplantilla, plantillaorigenid, etiquetasjson,
                       creadoporusuarioid, fechainicio, fechafin, publicadoen,
                       configuracionjson, creadoen
                FROM encuesta
                WHERE organizacionid = @OrganizacionId
                ORDER BY creadoen DESC
                """;
            return await db.CreateConnection.QueryAsync<EncuestaEntity>(sql, new { OrganizacionId = organizacionId });
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<EncuestaEntity?> ObtenerEncuestaPorId(Guid id, Guid organizacionId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            const string sql = """
                SELECT id, organizacionid, titulo, descripcion, version, estado,
                       esglobal, esplantilla, plantillaorigenid, etiquetasjson,
                       creadoporusuarioid, fechainicio, fechafin, publicadoen,
                       configuracionjson, creadoen
                FROM encuesta
                WHERE id = @Id AND organizacionid = @OrganizacionId
                """;
            return await db.CreateConnection.QueryFirstOrDefaultAsync<EncuestaEntity>(sql, new { Id = id, OrganizacionId = organizacionId });
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<bool> CrearEncuesta(EncuestaRequest request)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sql = """
                    INSERT INTO encuesta (organizacionid, titulo, descripcion, esglobal, esplantilla,
                                         plantillaorigenid, etiquetasjson, creadoporusuarioid,
                                         fechainicio, fechafin, configuracionjson)
                    VALUES (@OrganizacionId, @Titulo, @Descripcion, @EsGlobal, @EsPlantilla,
                            @PlantillaOrigenId, @EtiquetasJson::jsonb, @CreadoPorUsuarioId,
                            @FechaInicio, @FechaFin, @ConfiguracionJson::jsonb)
                    """;
                await db.CreateConnection.ExecuteAsync(sql, request, transaction: transaction);
                transaction.Commit();
                return true;
            }
            catch (ExceptionControlado ex) { transaction.Rollback(); throw new ExceptionControlado(ex.Message, ex); }
            catch (Exception ex) { transaction.Rollback(); throw new Exception(ex.Message, ex); }
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<bool> ActualizarEncuesta(EncuestaRequest request)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sql = """
                    UPDATE encuesta
                    SET titulo           = @Titulo,
                        descripcion      = @Descripcion,
                        esglobal         = @EsGlobal,
                        esplantilla      = @EsPlantilla,
                        plantillaorigenid = @PlantillaOrigenId,
                        etiquetasjson    = @EtiquetasJson::jsonb,
                        fechainicio      = @FechaInicio,
                        fechafin         = @FechaFin,
                        configuracionjson = @ConfiguracionJson::jsonb
                    WHERE id = @Id AND organizacionid = @OrganizacionId
                    """;
                await db.CreateConnection.ExecuteAsync(sql, request, transaction: transaction);
                transaction.Commit();
                return true;
            }
            catch (ExceptionControlado ex) { transaction.Rollback(); throw new ExceptionControlado(ex.Message, ex); }
            catch (Exception ex) { transaction.Rollback(); throw new Exception(ex.Message, ex); }
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<bool> EliminarEncuesta(Guid id, Guid organizacionId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sql = "DELETE FROM encuesta WHERE id = @Id AND organizacionid = @OrganizacionId";
                await db.CreateConnection.ExecuteAsync(sql, new { Id = id, OrganizacionId = organizacionId }, transaction: transaction);
                transaction.Commit();
                return true;
            }
            catch (ExceptionControlado ex) { transaction.Rollback(); throw new ExceptionControlado(ex.Message, ex); }
            catch (Exception ex) { transaction.Rollback(); throw new Exception(ex.Message, ex); }
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }
}
