namespace Encuesta.ModeloDatos;

public class CuentaUsuario
{
    public Guid Id { get; set; }
    public Guid OrganizacionId { get; set; }
    public Guid? EntidadId { get; set; }
    public string Correo { get; set; } = "";
    public string HashContrasena { get; set; } = "";
    public bool EsActivo { get; set; }
    public bool EsCuentaServicio { get; set; }
    public DateTime CreadoEn { get; set; }
}
