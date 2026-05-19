namespace Encuesta.ModeloDatos;

public class CuotaRespuestaRequest
{
    public Guid Id { get; set; }
    public Guid EncuestaId { get; set; }
    public Guid? EntidadId { get; set; }
    public int Limite { get; set; }
    public bool CerrarAlAlcanzar { get; set; } = true;
}
