namespace Encuesta.ModeloDatos;

public class ReglaEncuestaRequest
{
    public Guid Id { get; set; }
    public Guid EncuestaId { get; set; }
    public string ReglaJson { get; set; } = "";
}
