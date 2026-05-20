using Comun.BaseDatos;
using Comun.Herramientas;
using Dapper;
using Npgsql;
using System.Net.Sockets;
using DB = Comun.BaseDatos.Enumeradores;
using Encuesta.ModeloDatos;

namespace Encuesta.AccesoDatos;

public static class PublicoDAL
{
    public static async Task<EncuestaPublicaResponse?> ObtenerEncuestaPublica(Guid token)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();

            const string sqlInv = """
                SELECT i.id as invitacion_id, i.encuesta_id,
                       e.titulo, e.descripcion, e.version, e.estado as encuesta_estado
                FROM invitacion i
                JOIN encuesta e ON e.id = i.encuesta_id
                WHERE i.token_acceso = @Token
                  AND i.estado = 'PENDIENTE'
                  AND e.estado = 'PUBLICADA'
                  AND (i.vence_en IS NULL OR i.vence_en > NOW())
                """;

            var row = await db.CreateConnection.QueryFirstOrDefaultAsync(sqlInv, new { Token = token });
            if (row is null) return null;

            Guid invitacionId = row.invitacion_id;
            Guid encuestaId   = row.encuesta_id;

            const string sqlPreg = """
                SELECT id, seccion_id, tipo, titulo, descripcion, orden, es_obligatoria, configuracion_json
                FROM pregunta
                WHERE encuesta_id = @EncuestaId
                ORDER BY orden
                """;
            var preguntas = (await db.CreateConnection.QueryAsync<PreguntaPublicaResponse>(
                sqlPreg, new { EncuestaId = encuestaId })).ToList();

            if (preguntas.Count > 0)
            {
                var preguntaIds = preguntas.Select(p => p.Id).ToArray();
                const string sqlOpc = """
                    SELECT id, pregunta_id, etiqueta, valor, puntaje, orden
                    FROM opcion_pregunta
                    WHERE pregunta_id = ANY(@Ids)
                    ORDER BY orden
                    """;
                var opciones = await db.CreateConnection.QueryAsync<OpcionPreguntaResponse>(
                    sqlOpc, new { Ids = preguntaIds });

                var opcionesPorPregunta = opciones.GroupBy(o => o.PreguntaId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var p in preguntas)
                    p.Opciones = opcionesPorPregunta.TryGetValue(p.Id, out var lista) ? lista : new();
            }

            const string sqlSecciones = """
                SELECT id, titulo, descripcion, orden
                FROM seccion_encuesta
                WHERE encuesta_id = @EncuestaId
                ORDER BY orden
                """;
            var secciones = (await db.CreateConnection.QueryAsync<SeccionPublicaResponse>(
                sqlSecciones, new { EncuestaId = encuestaId })).ToList();

            const string sqlReglas = """
                SELECT regla_json::text
                FROM regla_encuesta
                WHERE encuesta_id = @EncuestaId
                """;
            var reglas = (await db.CreateConnection.QueryAsync<string>(
                sqlReglas, new { EncuestaId = encuestaId })).ToList();

            return new EncuestaPublicaResponse
            {
                InvitacionId = invitacionId,
                EncuestaId   = encuestaId,
                Titulo       = row.titulo,
                Descripcion  = row.descripcion,
                Version      = (int)row.version,
                Secciones    = secciones,
                Preguntas    = preguntas,
                Reglas       = reglas,
            };
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<bool> GuardarRespuesta(Guid token, SubmitRespuestaPublicaRequest request)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();
            using var transaction = db.CreateConnection.BeginTransaction();
            try
            {
                const string sqlInv = """
                    SELECT id, encuesta_id, version_encuesta
                    FROM (
                        SELECT i.id, i.encuesta_id, e.version AS version_encuesta
                        FROM invitacion i
                        JOIN encuesta e ON e.id = i.encuesta_id
                        WHERE i.token_acceso = @Token AND i.estado = 'PENDIENTE'
                    ) t
                    """;

                var inv = await db.CreateConnection.QueryFirstOrDefaultAsync(sqlInv, new { Token = token }, transaction);
                if (inv is null) throw new ExceptionControlado("Token de acceso inválido o ya utilizado.");

                Guid invId     = inv.id;
                Guid encId     = inv.encuesta_id;
                int  version   = (int)inv.version_encuesta;

                const string sqlResp = """
                    INSERT INTO respuesta (encuesta_id, version_encuesta, invitacion_id, canal, completado_en)
                    VALUES (@EncuestaId, @Version, @InvitacionId, 'ENLACE_PUBLICO', NOW())
                    RETURNING id
                    """;
                var respuestaId = await db.CreateConnection.ExecuteScalarAsync<Guid>(
                    sqlResp, new { EncuestaId = encId, Version = version, InvitacionId = invId }, transaction);

                const string sqlDetalle = """
                    INSERT INTO detalle_respuesta
                        (respuesta_id, pregunta_id, valor_texto, valor_numero, valor_booleano, valor_fecha, valor_json)
                    VALUES
                        (@RespuestaId, @PreguntaId, @ValorTexto, @ValorNumero, @ValorBooleano, @ValorFecha, @ValorJson::jsonb)
                    """;
                foreach (var d in request.Detalles)
                {
                    await db.CreateConnection.ExecuteAsync(sqlDetalle, new
                    {
                        RespuestaId  = respuestaId,
                        d.PreguntaId,
                        d.ValorTexto,
                        d.ValorNumero,
                        d.ValorBooleano,
                        d.ValorFecha,
                        ValorJson    = d.ValorJson,
                    }, transaction);
                }

                const string sqlMark = """
                    UPDATE invitacion SET estado = 'RESPONDIDA', respondido_en = NOW()
                    WHERE id = @Id
                    """;
                await db.CreateConnection.ExecuteAsync(sqlMark, new { Id = invId }, transaction);

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
