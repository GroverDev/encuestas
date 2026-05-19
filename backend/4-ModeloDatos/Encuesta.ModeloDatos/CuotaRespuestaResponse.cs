namespace Encuesta.ModeloDatos;

public class CuotaRespuestaResponse
{
    public Guid Id { get; set; }
    public Guid EncuestaId { get; set; }
    public Guid? EntidadId { get; set; }
    public int Limite { get; set; }
    public int TotalActual { get; set; }
    public bool CerrarAlAlcanzar { get; set; }
}
