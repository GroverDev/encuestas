namespace Encuesta.ModeloDatos;

public class AlcanceEncuestaRequest
{
    public Guid Id { get; set; }
    public Guid EncuestaId { get; set; }
    public Guid EntidadId { get; set; }
    public string TipoRelacion { get; set; } = "AMBITO";
    public bool IncluirDescendientes { get; set; } = false;
}
