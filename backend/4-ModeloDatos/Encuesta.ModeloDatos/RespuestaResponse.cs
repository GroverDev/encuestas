namespace Encuesta.ModeloDatos;

public class RespuestaResponse
{
    public Guid Id { get; set; }
    public Guid EncuestaId { get; set; }
    public int VersionEncuesta { get; set; }
    public Guid? InvitacionId { get; set; }
    public Guid? UsuarioRespondentId { get; set; }
    public string? Canal { get; set; }
    public Guid? UltimaPreguntaId { get; set; }
    public decimal PesoEstadistico { get; set; }
    public bool? ConsentimientoOtorgado { get; set; }
    public DateTime? FechaConsentimiento { get; set; }
    public DateTime? IniciadoEn { get; set; }
    public DateTime? CompletadoEn { get; set; }
    public string? InfoDispositivo { get; set; }
    public string? DireccionIp { get; set; }
}
