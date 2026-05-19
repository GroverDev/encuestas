namespace Encuesta.ModeloDatos;

public class CuentaUsuarioResponse
{
    public Guid Id { get; set; }
    public Guid OrganizacionId { get; set; }
    public Guid? EntidadId { get; set; }
    public string Correo { get; set; } = "";
    public bool EsActivo { get; set; }
    public DateTime CreadoEn { get; set; }
}
