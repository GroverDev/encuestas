namespace Encuesta.ModeloDatos;

public class SubmitRespuestaPublicaRequest
{
    public List<DetalleRespuestaPublicaItem> Detalles { get; set; } = new();
}

public class DetalleRespuestaPublicaItem
{
    public Guid PreguntaId { get; set; }
    public string? ValorTexto { get; set; }
    public decimal? ValorNumero { get; set; }
    public bool? ValorBooleano { get; set; }
    public DateTime? ValorFecha { get; set; }
    public string? ValorJson { get; set; }
}
