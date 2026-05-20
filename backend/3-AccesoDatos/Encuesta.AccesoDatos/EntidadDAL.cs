using Comun.BaseDatos;
using Comun.Herramientas;
using Dapper;
using Npgsql;
using System.Net.Sockets;
using DB = Comun.BaseDatos.Enumeradores;
using Encuesta.ModeloDatos;

namespace Encuesta.AccesoDatos;

public static class EntidadDAL
{
    public static async Task<IEnumerable<Entidad>> ObtenerEntidades(Guid organizacionId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            const string sql = """
                SELECT id, organizacion_id, tipo_entidad_id, entidad_padre_id,
                       nombre_visible, id_externo, es_activo, atributos_json, creado_en
                FROM entidad
                WHERE organizacion_id = @OrganizacionId
                ORDER BY nombre_visible
                """;
            return await db.CreateConnection.QueryAsync<Entidad>(sql, new { OrganizacionId = organizacionId });
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<Entidad?> ObtenerEntidadPorId(Guid id, Guid organizacionId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            const string sql = """
                SELECT id, organizacion_id, tipo_entidad_id, entidad_padre_id,
                       nombre_visible, id_externo, es_activo, atributos_json, creado_en
                FROM entidad
                WHERE id = @Id AND organizacion_id = @OrganizacionId
                """;
            return await db.CreateConnection.QueryFirstOrDefaultAsync<Entidad>(sql, new { Id = id, OrganizacionId = organizacionId });
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<Entidad?> ObtenerEntidadPorIdExterno(string idExterno, Guid organizacionId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            const string sql = """
                SELECT id, organizacion_id, tipo_entidad_id, entidad_padre_id,
                       nombre_visible, id_externo, es_activo, atributos_json, creado_en
                FROM entidad
                WHERE id_externo = @IdExterno AND organizacion_id = @OrganizacionId AND es_activo = TRUE
                LIMIT 1
                """;
            return await db.CreateConnection.QueryFirstOrDefaultAsync<Entidad>(sql, new { IdExterno = idExterno, OrganizacionId = organizacionId });
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<bool> CrearEntidad(EntidadRequest request)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sql = """
                    INSERT INTO entidad (organizacion_id, tipo_entidad_id, entidad_padre_id,
                                        nombre_visible, id_externo, atributos_json)
                    VALUES (@OrganizacionId, @TipoEntidadId, @EntidadPadreId,
                            @NombreVisible, @IdExterno, @AtributosJson::jsonb)
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

    public static async Task<bool> ActualizarEntidad(EntidadRequest request)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sql = """
                    UPDATE entidad
                    SET tipo_entidad_id  = @TipoEntidadId,
                        entidad_padre_id = @EntidadPadreId,
                        nombre_visible  = @NombreVisible,
                        id_externo      = @IdExterno,
                        atributos_json  = @AtributosJson::jsonb
                    WHERE id = @Id AND organizacion_id = @OrganizacionId
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

    public static async Task<bool> EliminarEntidad(Guid id, Guid organizacionId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sql = "UPDATE entidad SET es_activo = FALSE WHERE id = @Id AND organizacion_id = @OrganizacionId";
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

    public static async Task<Guid> SincronizarEntidad(EntidadRequest request)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                // INSERT … ON CONFLICT (id_externo, organizacion_id) DO UPDATE
                const string sql = """
                    INSERT INTO entidad (organizacion_id, tipo_entidad_id, entidad_padre_id,
                                        nombre_visible, id_externo, atributos_json, es_activo)
                    VALUES (@OrganizacionId, @TipoEntidadId, @EntidadPadreId,
                            @NombreVisible, @IdExterno, @AtributosJson::jsonb, TRUE)
                    ON CONFLICT (organizacion_id, id_externo)
                    DO UPDATE SET
                        tipo_entidad_id  = EXCLUDED.tipo_entidad_id,
                        entidad_padre_id = EXCLUDED.entidad_padre_id,
                        nombre_visible   = EXCLUDED.nombre_visible,
                        atributos_json   = EXCLUDED.atributos_json,
                        es_activo        = TRUE
                    RETURNING id
                    """;
                var id = await db.CreateConnection.ExecuteScalarAsync<Guid>(sql, request, transaction: transaction);
                transaction.Commit();
                return id;
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
