namespace Encuesta.ModeloDatos;

public class CuentaUsuarioRequest
{
    public Guid Id { get; set; }
    public Guid OrganizacionId { get; set; }
    public Guid? EntidadId { get; set; }
    public string Correo { get; set; } = "";
    public string Contrasena { get; set; } = "";
    public string HashContrasena { get; set; } = "";
}
