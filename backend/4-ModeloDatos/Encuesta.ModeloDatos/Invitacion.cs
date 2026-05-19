namespace Encuesta.ModeloDatos;

public class Invitacion
{
    public Guid Id { get; set; }
    public Guid EncuestaId { get; set; }
    public Guid? CuentaUsuarioId { get; set; }
    public string? CorreoDestino { get; set; }
    public Guid? EntidadEvaluadaId { get; set; }
    public Guid TokenAcceso { get; set; }
    public string Canal { get; set; } = "";
    public string Estado { get; set; } = "";
    public DateTime? EnviadoEn { get; set; }
    public DateTime? VenceEn { get; set; }
    public DateTime? RespondidoEn { get; set; }
}
