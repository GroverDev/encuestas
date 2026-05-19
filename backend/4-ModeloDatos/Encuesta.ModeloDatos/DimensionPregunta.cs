namespace Encuesta.ModeloDatos;

public class DimensionPregunta
{
    public Guid Id { get; set; }
    public Guid EncuestaId { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public decimal Peso { get; set; }
    public int Orden { get; set; }
}
