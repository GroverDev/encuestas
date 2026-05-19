using Comun.BaseDatos;
using Comun.Herramientas;
using Dapper;
using Encuesta.ModeloDatos;
using Npgsql;
using System.Net.Sockets;
using DB = Comun.BaseDatos.Enumeradores;

namespace Encuesta.AccesoDatos;

public static class PreguntaDAL
{
    public static async Task<IEnumerable<Pregunta>> ObtenerPreguntas(Guid encuestaId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            const string sql = """
                SELECT id, encuestaid, seccionid, dimensionid, tipo, titulo, descripcion,
                       orden, peso, esobligatoria, configuracionjson, creadoen
                FROM pregunta
                WHERE encuestaid = @EncuestaId
                ORDER BY orden
                """;
            return await db.CreateConnection.QueryAsync<Pregunta>(sql, new { EncuestaId = encuestaId });
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<Pregunta?> ObtenerPreguntaPorId(Guid id, Guid encuestaId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            const string sql = """
                SELECT id, encuestaid, seccionid, dimensionid, tipo, titulo, descripcion,
                       orden, peso, esobligatoria, configuracionjson, creadoen
                FROM pregunta
                WHERE id = @Id AND encuestaid = @EncuestaId
                """;
            return await db.CreateConnection.QueryFirstOrDefaultAsync<Pregunta>(sql, new { Id = id, EncuestaId = encuestaId });
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<bool> CrearPregunta(PreguntaRequest request)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sql = """
                    INSERT INTO pregunta (encuestaid, seccionid, dimensionid, tipo, titulo,
                                         descripcion, orden, peso, esobligatoria, configuracionjson)
                    VALUES (@EncuestaId, @SeccionId, @DimensionId, @Tipo, @Titulo,
                            @Descripcion, @Orden, @Peso, @EsObligatoria, @ConfiguracionJson::jsonb)
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

    public static async Task<bool> ActualizarPregunta(PreguntaRequest request)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sql = """
                    UPDATE pregunta
                    SET seccionid        = @SeccionId,
                        dimensionid      = @DimensionId,
                        tipo             = @Tipo,
                        titulo           = @Titulo,
                        descripcion      = @Descripcion,
                        orden            = @Orden,
                        peso             = @Peso,
                        esobligatoria    = @EsObligatoria,
                        configuracionjson = @ConfiguracionJson::jsonb
                    WHERE id = @Id AND encuestaid = @EncuestaId
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

    public static async Task<bool> EliminarPregunta(Guid id, Guid encuestaId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sql = "DELETE FROM pregunta WHERE id = @Id AND encuestaid = @EncuestaId";
                await db.CreateConnection.ExecuteAsync(sql, new { Id = id, EncuestaId = encuestaId }, transaction: transaction);
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
