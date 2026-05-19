namespace Encuesta.ModeloDatos;

public class NotificacionEnvioResponse
{
    public Guid Id { get; set; }
    public Guid InvitacionId { get; set; }
    public string Tipo { get; set; } = "";
    public string Canal { get; set; } = "";
    public string Destinatario { get; set; } = "";
    public string Estado { get; set; } = "";
    public int IntentosEnvio { get; set; }
    public DateTime? EnviadoEn { get; set; }
    public DateTime? EntregadoEn { get; set; }
    public string? ErrorDetalle { get; set; }
    public DateTime CreadoEn { get; set; }
}
