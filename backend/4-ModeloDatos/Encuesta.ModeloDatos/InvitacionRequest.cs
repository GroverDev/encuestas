namespace Encuesta.ModeloDatos;

public class InvitacionRequest
{
    public Guid Id { get; set; }
    public Guid EncuestaId { get; set; }
    public Guid? CuentaUsuarioId { get; set; }
    public string? CorreoDestino { get; set; }
    public Guid? EntidadEvaluadaId { get; set; }
    public string? EntidadEvaluadaIdExterno { get; set; }
    public string Canal { get; set; } = "EMAIL";
    public string Estado { get; set; } = "PENDIENTE";
    public DateTime? VenceEn { get; set; }
}
