using Comun.BaseDatos;
using Comun.Herramientas;
using Dapper;
using Npgsql;
using System.Net.Sockets;
using DB = Comun.BaseDatos.Enumeradores;
using Encuesta.ModeloDatos;

namespace Encuesta.AccesoDatos;

public static class ComentarioDAL
{
    public static async Task<IEnumerable<Comentario>> ObtenerComentariosPorEntidad(Guid entidadId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            const string sql = """
                SELECT id, entidad_id, respuesta_id, usuario_id, texto_comentario, creado_en
                FROM comentario
                WHERE entidad_id = @EntidadId
                ORDER BY creado_en DESC
                """;
            return await db.CreateConnection.QueryAsync<Comentario>(sql, new { EntidadId = entidadId });
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<IEnumerable<Comentario>> ObtenerComentariosPorRespuesta(Guid respuestaId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            const string sql = """
                SELECT id, entidad_id, respuesta_id, usuario_id, texto_comentario, creado_en
                FROM comentario
                WHERE respuesta_id = @RespuestaId
                ORDER BY creado_en DESC
                """;
            return await db.CreateConnection.QueryAsync<Comentario>(sql, new { RespuestaId = respuestaId });
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<Comentario?> ObtenerComentarioPorId(Guid id)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            const string sql = """
                SELECT id, entidad_id, respuesta_id, usuario_id, texto_comentario, creado_en
                FROM comentario
                WHERE id = @Id
                """;
            return await db.CreateConnection.QueryFirstOrDefaultAsync<Comentario>(sql, new { Id = id });
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<bool> CrearComentario(ComentarioRequest request)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sql = """
                    INSERT INTO comentario (entidad_id, respuesta_id, usuario_id, texto_comentario)
                    VALUES (@EntidadId, @RespuestaId, @UsuarioId, @TextoComentario)
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

    public static async Task<bool> ActualizarComentario(ComentarioRequest request)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sql = """
                    UPDATE comentario
                    SET texto_comentario = @TextoComentario
                    WHERE id = @Id
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

    public static async Task<bool> EliminarComentario(Guid id)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sql = "DELETE FROM comentario WHERE id = @Id";
                await db.CreateConnection.ExecuteAsync(sql, new { Id = id }, transaction: transaction);
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
