namespace Encuesta.ModeloDatos;

public class Rol
{
    public Guid Id { get; set; }
    public Guid OrganizacionId { get; set; }
    public string Nombre { get; set; } = "";
}
