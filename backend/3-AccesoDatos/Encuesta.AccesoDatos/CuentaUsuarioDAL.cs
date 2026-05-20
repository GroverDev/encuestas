using Comun.BaseDatos;
using Comun.Herramientas;
using Dapper;
using Npgsql;
using System.Net.Sockets;
using DB = Comun.BaseDatos.Enumeradores;
using Encuesta.ModeloDatos;

namespace Encuesta.AccesoDatos;

public static class CuentaUsuarioDAL
{
    public static async Task<IEnumerable<CuentaUsuario>> ObtenerCuentasUsuario(Guid organizacionId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            const string sql = """
                SELECT id, organizacion_id, entidad_id, correo, hash_contrasena, es_activo, es_cuenta_servicio, creado_en
                FROM cuenta_usuario
                WHERE organizacion_id = @OrganizacionId
                ORDER BY correo
                """;
            return await db.CreateConnection.QueryAsync<CuentaUsuario>(sql, new { OrganizacionId = organizacionId });
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<CuentaUsuario?> ObtenerCuentaUsuarioPorId(Guid id, Guid organizacionId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            const string sql = """
                SELECT id, organizacion_id, entidad_id, correo, hash_contrasena, es_activo, es_cuenta_servicio, creado_en
                FROM cuenta_usuario
                WHERE id = @Id AND organizacion_id = @OrganizacionId
                """;
            return await db.CreateConnection.QueryFirstOrDefaultAsync<CuentaUsuario>(sql, new { Id = id, OrganizacionId = organizacionId });
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<CuentaUsuario?> ObtenerCuentaUsuarioPorCorreo(string correo)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            const string sql = """
                SELECT id, organizacion_id, entidad_id, correo, hash_contrasena, es_activo, es_cuenta_servicio, creado_en
                FROM cuenta_usuario
                WHERE correo = @Correo AND es_activo = TRUE
                """;
            return await db.CreateConnection.QueryFirstOrDefaultAsync<CuentaUsuario>(sql, new { Correo = correo });
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<bool> CrearCuentaUsuario(CuentaUsuarioRequest request)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sql = """
                    INSERT INTO cuenta_usuario (organizacion_id, entidad_id, correo, hash_contrasena, es_cuenta_servicio)
                    VALUES (@OrganizacionId, @EntidadId, @Correo, @HashContrasena, @EsCuentaServicio)
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

    public static async Task<bool> ActualizarCuentaUsuario(CuentaUsuarioRequest request)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sql = """
                    UPDATE cuenta_usuario
                    SET entidad_id         = @EntidadId,
                        correo             = @Correo,
                        hash_contrasena    = @HashContrasena,
                        es_cuenta_servicio = @EsCuentaServicio
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

    public static async Task<bool> EliminarCuentaUsuario(Guid id, Guid organizacionId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sql = "UPDATE cuenta_usuario SET es_activo = FALSE WHERE id = @Id AND organizacion_id = @OrganizacionId";
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
