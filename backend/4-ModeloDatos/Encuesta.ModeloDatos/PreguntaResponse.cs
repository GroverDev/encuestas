namespace Encuesta.ModeloDatos;

public class PreguntaResponse
{
    public Guid Id { get; set; }
    public Guid EncuestaId { get; set; }
    public Guid? SeccionId { get; set; }
    public Guid? DimensionId { get; set; }
    public string Tipo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string? Descripcion { get; set; }
    public int Orden { get; set; }
    public decimal Peso { get; set; }
    public bool EsObligatoria { get; set; }
    public string? ConfiguracionJson { get; set; }
    public DateTime CreadoEn { get; set; }
}
