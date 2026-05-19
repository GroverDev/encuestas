namespace Encuesta.ModeloDatos;

public class AdjuntoRequest
{
    public Guid Id { get; set; }
    public Guid EntidadId { get; set; }
    public string? NombreArchivo { get; set; }
    public string UrlArchivo { get; set; } = "";
}
