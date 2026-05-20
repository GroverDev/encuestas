namespace Encuesta.ModeloDatos;

public class EstadisticasEncuestaResponse
{
    public int TotalRespuestas { get; set; }
    public List<EstadisticasPreguntaResponse> Preguntas { get; set; } = new();
}

public class EstadisticasPreguntaResponse
{
    public Guid PreguntaId { get; set; }
    public string Tipo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public int TotalRespuestas { get; set; }
    public decimal? Promedio { get; set; }
    public decimal? Minimo { get; set; }
    public decimal? Maximo { get; set; }
    public decimal? PuntajeNps { get; set; }
    public List<ConteoOpcion> Conteos { get; set; } = new();
    public List<string> TextosLibres { get; set; } = new();
}

public class ConteoOpcion
{
    public string Valor { get; set; } = "";
    public string Etiqueta { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal Porcentaje { get; set; }
}

public class ResumenEntidadResponse
{
    public Guid EntidadId { get; set; }
    public string NombreEntidad { get; set; } = "";
    public Guid? EntidadPadreId { get; set; }
    public string? IdExterno { get; set; }
    public string TipoEntidad { get; set; } = "";
    public int TotalRespuestas { get; set; }
    public List<MetricaEntidad> Metricas { get; set; } = new();
}

public class MetricaEntidad
{
    public Guid PreguntaId { get; set; }
    public string Titulo { get; set; } = "";
    public string Tipo { get; set; } = "";
    public decimal? Promedio { get; set; }
    public decimal? PuntajeNps { get; set; }
}
