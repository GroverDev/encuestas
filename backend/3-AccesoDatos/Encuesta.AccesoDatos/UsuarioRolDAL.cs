using Comun.BaseDatos;
using Comun.Herramientas;
using Dapper;
using Npgsql;
using System.Net.Sockets;
using DB = Comun.BaseDatos.Enumeradores;
using Encuesta.ModeloDatos;

namespace Encuesta.AccesoDatos;

public static class UsuarioRolDAL
{
    public static async Task<IEnumerable<UsuarioRol>> ObtenerUsuarioRoles(Guid usuarioId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            const string sql = "SELECT usuarioid, rolid FROM usuariorol WHERE usuarioid = @UsuarioId";
            return await db.CreateConnection.QueryAsync<UsuarioRol>(sql, new { UsuarioId = usuarioId });
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<bool> AsignarRolUsuario(UsuarioRolRequest request)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sql = """
                    INSERT INTO usuariorol (usuarioid, rolid)
                    VALUES (@UsuarioId, @RolId)
                    ON CONFLICT DO NOTHING
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

    public static async Task<bool> RemoverRolUsuario(Guid usuarioId, Guid rolId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sql = "DELETE FROM usuariorol WHERE usuarioid = @UsuarioId AND rolid = @RolId";
                await db.CreateConnection.ExecuteAsync(sql, new { UsuarioId = usuarioId, RolId = rolId }, transaction: transaction);
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
