namespace Encuesta.ModeloDatos;

public class ReglaEncuesta
{
    public Guid Id { get; set; }
    public Guid EncuestaId { get; set; }
    public string ReglaJson { get; set; } = "";
}
