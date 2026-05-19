namespace Encuesta.ModeloDatos;

public class OpcionPreguntaRequest
{
    public Guid Id { get; set; }
    public Guid PreguntaId { get; set; }
    public string Etiqueta { get; set; } = "";
    public string Valor { get; set; } = "";
    public decimal? Puntaje { get; set; }
    public int Orden { get; set; }
}
