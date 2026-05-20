using Comun.Herramientas;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;
using System.Text.Json;

namespace Encuesta.LogicaNegocios;

public static class EstadisticasBLL
{
    public static async Task<Respuesta<List<ResumenEntidadResponse>>> ObtenerResumenPorEntidad(Guid encuestaId)
    {
        var response = new Respuesta<List<ResumenEntidadResponse>>();
        try
        {
            response.Datos = await EstadisticasDAL.ObtenerResumenPorEntidad(encuestaId);
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    public static async Task<Respuesta<EstadisticasEncuestaResponse>> ObtenerEstadisticas(Guid encuestaId, Guid? entidadId = null)
    {
        var response = new Respuesta<EstadisticasEncuestaResponse>();
        try
        {
            var (totalRespuestas, preguntas, opciones, detalles) =
                await EstadisticasDAL.ObtenerDatosEstadisticas(encuestaId, entidadId);

            var opcionesPorPregunta = opciones.GroupBy(o => o.PreguntaId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var detallesPorPregunta = detalles.GroupBy(d => d.PreguntaId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var estadisticas = preguntas.Select(p =>
            {
                var dets = detallesPorPregunta.TryGetValue(p.Id, out var d) ? d : new();
                var opts = opcionesPorPregunta.TryGetValue(p.Id, out var o) ? o : new();
                return Agregar(p, dets, opts, totalRespuestas);
            }).ToList();

            response.Datos = new EstadisticasEncuestaResponse
            {
                TotalRespuestas = totalRespuestas,
                Preguntas       = estadisticas,
            };
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }

    private static EstadisticasPreguntaResponse Agregar(
        PreguntaRaw p, List<DetalleRaw> dets, List<OpcionRaw> opts, int totalResp)
    {
        var r = new EstadisticasPreguntaResponse
        {
            PreguntaId     = p.Id,
            Tipo           = p.Tipo,
            Titulo         = p.Titulo,
            TotalRespuestas = dets.Count,
        };

        switch (p.Tipo)
        {
            case "TEXTO":
                r.TextosLibres = dets
                    .Where(d => !string.IsNullOrWhiteSpace(d.ValorTexto))
                    .Select(d => d.ValorTexto!)
                    .ToList();
                break;

            case "NUMERO":
            case "ESCALA":
            case "CALIFICACION":
                var nums = dets.Where(d => d.ValorNumero.HasValue).Select(d => d.ValorNumero!.Value).ToList();
                if (nums.Count > 0)
                {
                    r.Promedio = Math.Round(nums.Average(), 2);
                    r.Minimo   = nums.Min();
                    r.Maximo   = nums.Max();
                }
                break;

            case "BOOLEANO":
                var si = dets.Count(d => d.ValorBooleano == true);
                var no = dets.Count(d => d.ValorBooleano == false);
                var totalBool = si + no;
                r.Conteos = new List<ConteoOpcion>
                {
                    new() { Valor = "true",  Etiqueta = "Sí", Cantidad = si, Porcentaje = Pct(si, totalBool) },
                    new() { Valor = "false", Etiqueta = "No", Cantidad = no, Porcentaje = Pct(no, totalBool) },
                };
                break;

            case "SELECCION_UNICA":
                r.Conteos = ContarPorTexto(dets, opts, dets.Count);
                break;

            case "SELECCION_MULTIPLE":
                r.Conteos = ContarDesdeJsonArray(dets, opts, totalResp);
                break;

            case "NPS":
                var npsDets = dets.Where(d => d.ValorNumero.HasValue).ToList();
                if (npsDets.Count > 0)
                {
                    var npsNums = npsDets.Select(d => (int)d.ValorNumero!.Value).ToList();
                    r.Promedio  = Math.Round((decimal)npsNums.Average(), 1);
                    var total   = npsNums.Count;
                    var detractores = npsNums.Count(n => n <= 6);
                    var neutrales   = npsNums.Count(n => n is 7 or 8);
                    var promotores  = npsNums.Count(n => n >= 9);
                    r.PuntajeNps = Math.Round(Pct(promotores, total) - Pct(detractores, total), 1);
                    r.Conteos = new List<ConteoOpcion>
                    {
                        new() { Valor = "detractores", Etiqueta = "Detractores (0–6)", Cantidad = detractores, Porcentaje = Pct(detractores, total) },
                        new() { Valor = "neutrales",   Etiqueta = "Neutrales (7–8)",   Cantidad = neutrales,   Porcentaje = Pct(neutrales,   total) },
                        new() { Valor = "promotores",  Etiqueta = "Promotores (9–10)", Cantidad = promotores,  Porcentaje = Pct(promotores,  total) },
                    };
                }
                break;

            case "RANKING":
                r.Conteos = ContarDesdeJsonArray(dets, opts, totalResp);
                break;

            case "MATRIZ":
                r.Conteos = ContarDesdeJsonObject(dets, opts, totalResp);
                break;

            case "FECHA":
                r.TotalRespuestas = dets.Count(d => d.ValorFecha.HasValue);
                break;
        }

        return r;
    }

    private static List<ConteoOpcion> ContarPorTexto(List<DetalleRaw> dets, List<OpcionRaw> opts, int total)
    {
        var conteo = dets
            .Where(d => !string.IsNullOrEmpty(d.ValorTexto))
            .GroupBy(d => d.ValorTexto!)
            .ToDictionary(g => g.Key, g => g.Count());

        return opts.Count > 0
            ? opts.Select(o => new ConteoOpcion
            {
                Valor     = o.Valor,
                Etiqueta  = o.Etiqueta,
                Cantidad  = conteo.TryGetValue(o.Valor, out var c) ? c : 0,
                Porcentaje = Pct(conteo.TryGetValue(o.Valor, out var c2) ? c2 : 0, total),
            }).ToList()
            : conteo.Select(kv => new ConteoOpcion
            {
                Valor      = kv.Key,
                Etiqueta   = kv.Key,
                Cantidad   = kv.Value,
                Porcentaje = Pct(kv.Value, total),
            }).ToList();
    }

    private static List<ConteoOpcion> ContarDesdeJsonArray(List<DetalleRaw> dets, List<OpcionRaw> opts, int total)
    {
        var conteo = new Dictionary<string, int>();
        foreach (var d in dets.Where(d => !string.IsNullOrEmpty(d.ValorJson)))
        {
            try
            {
                var arr = JsonSerializer.Deserialize<List<string>>(d.ValorJson!) ?? new();
                foreach (var v in arr)
                    conteo[v] = conteo.TryGetValue(v, out var c) ? c + 1 : 1;
            }
            catch { }
        }

        return opts.Count > 0
            ? opts.Select(o => new ConteoOpcion
            {
                Valor      = o.Valor,
                Etiqueta   = o.Etiqueta,
                Cantidad   = conteo.TryGetValue(o.Valor, out var c) ? c : 0,
                Porcentaje = Pct(conteo.TryGetValue(o.Valor, out var c2) ? c2 : 0, total),
            }).ToList()
            : conteo.Select(kv => new ConteoOpcion
            {
                Valor      = kv.Key,
                Etiqueta   = kv.Key,
                Cantidad   = kv.Value,
                Porcentaje = Pct(kv.Value, total),
            }).ToList();
    }

    private static List<ConteoOpcion> ContarDesdeJsonObject(List<DetalleRaw> dets, List<OpcionRaw> opts, int total)
    {
        var conteo = new Dictionary<string, int>();
        foreach (var d in dets.Where(d => !string.IsNullOrEmpty(d.ValorJson)))
        {
            try
            {
                var obj = JsonSerializer.Deserialize<Dictionary<string, string>>(d.ValorJson!) ?? new();
                foreach (var kv in obj)
                {
                    var key = $"{kv.Key} → {kv.Value}";
                    conteo[key] = conteo.TryGetValue(key, out var c) ? c + 1 : 1;
                }
            }
            catch { }
        }

        return conteo
            .OrderByDescending(kv => kv.Value)
            .Select(kv => new ConteoOpcion
            {
                Valor      = kv.Key,
                Etiqueta   = kv.Key,
                Cantidad   = kv.Value,
                Porcentaje = Pct(kv.Value, total),
            }).ToList();
    }

    private static decimal Pct(int parte, int total) =>
        total == 0 ? 0 : Math.Round((decimal)parte / total * 100, 1);
}
