namespace Encuesta.ModeloDatos;

public class NotificacionEnvioRequest
{
    public Guid InvitacionId { get; set; }
    public string Tipo { get; set; } = "";
    public string Canal { get; set; } = "";
    public string Destinatario { get; set; } = "";
}
