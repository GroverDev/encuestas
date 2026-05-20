using Comun.BaseDatos;
using Comun.Herramientas;
using Dapper;
using Npgsql;
using System.Net.Sockets;
using DB = Comun.BaseDatos.Enumeradores;
using Encuesta.ModeloDatos;

namespace Encuesta.AccesoDatos;

public record PreguntaRaw(Guid Id, string Tipo, string Titulo, int Orden, string? ConfiguracionJson);
public record OpcionRaw(Guid PreguntaId, string Valor, string Etiqueta);
public record DetalleRaw(Guid PreguntaId, string? ValorTexto, decimal? ValorNumero, bool? ValorBooleano, DateTime? ValorFecha, string? ValorJson);

public static class EstadisticasDAL
{
        public static async Task<(int TotalRespuestas, List<PreguntaRaw> Preguntas, List<OpcionRaw> Opciones, List<DetalleRaw> Detalles)>
        ObtenerDatosEstadisticas(Guid encuestaId, Guid? entidadId = null)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();

            // CTE recursivo que incluye la entidad y todos sus descendientes
            string cteDescendientes = entidadId.HasValue ? """
                WITH RECURSIVE descendientes AS (
                    SELECT id FROM entidad WHERE id = @EntidadId
                    UNION ALL
                    SELECT e.id FROM entidad e
                    JOIN descendientes d ON e.entidad_padre_id = d.id
                )
                """ : "";

            string filtroEntidad = entidadId.HasValue
                ? "AND i.entidad_evaluada_id IN (SELECT id FROM descendientes)"
                : "";

            string sqlTotal = entidadId.HasValue
                ? $"""
                  {cteDescendientes}
                  SELECT COUNT(DISTINCT r.id)::int FROM respuesta r
                  JOIN invitacion i ON i.id = r.invitacion_id
                  WHERE r.encuesta_id = @Id {filtroEntidad}
                  """
                : "SELECT COUNT(*)::int FROM respuesta WHERE encuesta_id = @Id";

            var totalRespuestas = await db.CreateConnection.ExecuteScalarAsync<int>(
                sqlTotal, new { Id = encuestaId, EntidadId = entidadId });

            var preguntas = (await db.CreateConnection.QueryAsync<PreguntaRaw>("""
                SELECT id, tipo, titulo, orden, configuracion_json
                FROM pregunta WHERE encuesta_id = @Id ORDER BY orden
                """, new { Id = encuestaId })).ToList();

            List<OpcionRaw> opciones = new();
            List<DetalleRaw> detalles = new();

            if (preguntas.Count > 0)
            {
                var ids = preguntas.Select(p => p.Id).ToArray();

                opciones = (await db.CreateConnection.QueryAsync<OpcionRaw>("""
                    SELECT pregunta_id, valor, etiqueta
                    FROM opcion_pregunta WHERE pregunta_id = ANY(@Ids) ORDER BY orden
                    """, new { Ids = ids })).ToList();

                string sqlDetalles = entidadId.HasValue
                    ? $"""
                      {cteDescendientes}
                      SELECT dr.pregunta_id, dr.valor_texto, dr.valor_numero,
                             dr.valor_booleano, dr.valor_fecha, dr.valor_json
                      FROM detalle_respuesta dr
                      JOIN respuesta r ON r.id = dr.respuesta_id
                      JOIN invitacion i ON i.id = r.invitacion_id
                      WHERE r.encuesta_id = @Id {filtroEntidad}
                      """
                    : """
                      SELECT dr.pregunta_id, dr.valor_texto, dr.valor_numero,
                             dr.valor_booleano, dr.valor_fecha, dr.valor_json
                      FROM detalle_respuesta dr
                      JOIN respuesta r ON r.id = dr.respuesta_id
                      WHERE r.encuesta_id = @Id
                      """;

                detalles = (await db.CreateConnection.QueryAsync<DetalleRaw>(
                    sqlDetalles, new { Id = encuestaId, EntidadId = entidadId })).ToList();
            }

            return (totalRespuestas, preguntas, opciones, detalles);
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }

    public static async Task<List<ResumenEntidadResponse>> ObtenerResumenPorEntidad(Guid encuestaId)
    {
        var db = new DapperDb(DB.GestorDB.POSTGRESQL, DB.Sistema.ENCUESTA, DB.BaseDeDatos.LOCAL);
        try
        {
            db.CreateConnection.Open();

            // CTE recursivo que sube por la jerarquía desde las hojas (entidades con respuestas directas)
            // hasta la empresa, generando pares (ancestor_id, leaf_id) para agregar respuestas hacia arriba.
            const string cteAncestros = """
                WITH RECURSIVE entity_ancestors AS (
                    SELECT DISTINCT
                        i.entidad_evaluada_id AS ancestor_id,
                        i.entidad_evaluada_id AS leaf_id
                    FROM invitacion i
                    JOIN respuesta r ON r.invitacion_id = i.id AND r.encuesta_id = @EncuestaId
                    WHERE i.entidad_evaluada_id IS NOT NULL
                    UNION
                    SELECT e.entidad_padre_id, ea.leaf_id
                    FROM entity_ancestors ea
                    JOIN entidad e ON e.id = ea.ancestor_id
                    WHERE e.entidad_padre_id IS NOT NULL
                )
                """;

            var entidades = (await db.CreateConnection.QueryAsync<ResumenEntidadResponse>(
                cteAncestros + """
                SELECT
                    ea.ancestor_id                  AS EntidadId,
                    e.nombre_visible                AS NombreEntidad,
                    e.entidad_padre_id              AS EntidadPadreId,
                    e.id_externo                    AS IdExterno,
                    COALESCE(te.nombre, 'Sin tipo') AS TipoEntidad,
                    COUNT(DISTINCT r.id)::int        AS TotalRespuestas
                FROM entity_ancestors ea
                JOIN entidad e ON e.id = ea.ancestor_id
                LEFT JOIN tipo_entidad te ON te.id = e.tipo_entidad_id
                JOIN invitacion i ON i.entidad_evaluada_id = ea.leaf_id AND i.encuesta_id = @EncuestaId
                JOIN respuesta r ON r.invitacion_id = i.id AND r.encuesta_id = @EncuestaId
                GROUP BY ea.ancestor_id, e.nombre_visible, e.entidad_padre_id, e.id_externo, te.nombre
                ORDER BY e.nombre_visible
                """, new { EncuestaId = encuestaId })).ToList();

            if (entidades.Count == 0) return entidades;

            var metricas = (await db.CreateConnection.QueryAsync<dynamic>(
                cteAncestros + """
                SELECT
                    ea.ancestor_id                       AS EntidadId,
                    p.id                                 AS PreguntaId,
                    p.titulo                             AS Titulo,
                    p.tipo                               AS Tipo,
                    AVG(dr.valor_numero)::numeric(10,2)  AS Promedio
                FROM entity_ancestors ea
                JOIN invitacion i ON i.entidad_evaluada_id = ea.leaf_id AND i.encuesta_id = @EncuestaId
                JOIN respuesta r ON r.invitacion_id = i.id AND r.encuesta_id = @EncuestaId
                JOIN detalle_respuesta dr ON dr.respuesta_id = r.id
                JOIN pregunta p ON p.id = dr.pregunta_id
                WHERE p.tipo IN ('CALIFICACION', 'ESCALA', 'NUMERO', 'NPS')
                  AND dr.valor_numero IS NOT NULL
                GROUP BY ea.ancestor_id, p.id, p.titulo, p.tipo, p.orden
                ORDER BY p.orden
                """, new { EncuestaId = encuestaId })).ToList();

            var entidadMap = entidades.ToDictionary(e => e.EntidadId);

            foreach (var m in metricas)
            {
                Guid eid = m.entidadid;
                if (!entidadMap.TryGetValue(eid, out var ent)) continue;

                decimal? promedio = m.promedio;
                decimal? nps = null;

                if ((string)m.tipo == "NPS" && promedio.HasValue)
                {
                    nps = promedio;
                    promedio = null;
                }

                ent.Metricas.Add(new MetricaEntidad
                {
                    PreguntaId = m.preguntaid,
                    Titulo     = m.titulo,
                    Tipo       = m.tipo,
                    Promedio   = promedio,
                    PuntajeNps = nps,
                });
            }

            return entidades;
        }
        catch (NpgsqlException ex) when (ex.InnerException is SocketException) { throw new ExceptionControlado("El servidor de la base de datos está caído o inaccesible."); }
        catch (NpgsqlException ex) { throw new ExceptionControlado($"Error al interactuar con la base de datos: {ex.Message}"); }
        catch (ExceptionControlado ex) { throw new ExceptionControlado(ex.Message, ex); }
        catch (Exception ex) { throw new Exception(ex.Message, ex); }
        finally { db.CreateConnection.Close(); }
    }
}
