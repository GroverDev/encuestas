namespace Encuesta.ModeloDatos;

public class ObjetivoRespuesta
{
    public Guid Id { get; set; }
    public Guid RespuestaId { get; set; }
    public Guid EntidadId { get; set; }
    public string TipoRelacion { get; set; } = "";
}
