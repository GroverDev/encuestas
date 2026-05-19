namespace Encuesta.ModeloDatos;

public class DimensionPreguntaRequest
{
    public Guid Id { get; set; }
    public Guid EncuestaId { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public decimal Peso { get; set; } = 1;
    public int Orden { get; set; }
}
