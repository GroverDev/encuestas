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
                SELECT id, organizacion_id, titulo, descripcion, version, estado,
                       es_global, es_plantilla, plantilla_origen_id, etiquetas_json,
                       creado_por_usuario_id, fecha_inicio, fecha_fin, publicado_en,
                       configuracion_json, creado_en
                FROM encuesta
                WHERE organizacion_id = @OrganizacionId
                ORDER BY creado_en DESC
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
                SELECT id, organizacion_id, titulo, descripcion, version, estado,
                       es_global, es_plantilla, plantilla_origen_id, etiquetas_json,
                       creado_por_usuario_id, fecha_inicio, fecha_fin, publicado_en,
                       configuracion_json, creado_en
                FROM encuesta
                WHERE id = @Id AND organizacion_id = @OrganizacionId
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
                    INSERT INTO encuesta (organizacion_id, titulo, descripcion, es_global, es_plantilla,
                                         plantilla_origen_id, etiquetas_json, creado_por_usuario_id,
                                         fecha_inicio, fecha_fin, configuracion_json)
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
                        es_global         = @EsGlobal,
                        es_plantilla      = @EsPlantilla,
                        plantilla_origen_id = @PlantillaOrigenId,
                        etiquetas_json    = @EtiquetasJson::jsonb,
                        fecha_inicio      = @FechaInicio,
                        fecha_fin         = @FechaFin,
                        configuracion_json = @ConfiguracionJson::jsonb
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

    public static async Task<bool> CambiarEstadoEncuesta(Guid id, Guid organizacionId, string nuevoEstado)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                var sql = nuevoEstado == "PUBLICADA"
                    ? """
                      UPDATE encuesta
                      SET estado = 'PUBLICADA', publicado_en = NOW()
                      WHERE id = @Id AND organizacion_id = @OrganizacionId AND estado = 'BORRADOR'
                      """
                    : """
                      UPDATE encuesta
                      SET estado = @NuevoEstado
                      WHERE id = @Id AND organizacion_id = @OrganizacionId
                      """;
                var rows = await db.CreateConnection.ExecuteAsync(sql, new { Id = id, OrganizacionId = organizacionId, NuevoEstado = nuevoEstado }, transaction: transaction);
                transaction.Commit();
                return rows > 0;
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
                const string sql = "DELETE FROM encuesta WHERE id = @Id AND organizacion_id = @OrganizacionId";
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
