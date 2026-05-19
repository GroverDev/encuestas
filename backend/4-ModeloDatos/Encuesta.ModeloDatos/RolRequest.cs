namespace Encuesta.ModeloDatos;

public class RolRequest
{
    public Guid Id { get; set; }
    public Guid OrganizacionId { get; set; }
    public string Nombre { get; set; } = "";
}
