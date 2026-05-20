namespace Encuesta.ModeloDatos;

public class CambiarEstadoEncuestaRequest
{
    public Guid Id { get; set; }
    public string NuevoEstado { get; set; } = "";
}
