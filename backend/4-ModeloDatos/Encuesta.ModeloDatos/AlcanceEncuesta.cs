namespace Encuesta.ModeloDatos;

public class AlcanceEncuesta
{
    public Guid Id { get; set; }
    public Guid EncuestaId { get; set; }
    public Guid EntidadId { get; set; }
    public string TipoRelacion { get; set; } = "";
    public bool IncluirDescendientes { get; set; }
}
