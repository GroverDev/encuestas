namespace Encuesta.ModeloDatos;

public class DetalleRespuestaRequest
{
    public Guid Id { get; set; }
    public Guid RespuestaId { get; set; }
    public Guid PreguntaId { get; set; }
    public string? ValorTexto { get; set; }
    public decimal? ValorNumero { get; set; }
    public bool? ValorBooleano { get; set; }
    public DateTime? ValorFecha { get; set; }
    public string? ValorJson { get; set; }
}
