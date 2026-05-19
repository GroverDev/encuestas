namespace Encuesta.ModeloDatos;

public class SeccionEncuestaRequest
{
    public Guid Id { get; set; }
    public Guid EncuestaId { get; set; }
    public string Titulo { get; set; } = "";
    public string? Descripcion { get; set; }
    public int Orden { get; set; }
}
