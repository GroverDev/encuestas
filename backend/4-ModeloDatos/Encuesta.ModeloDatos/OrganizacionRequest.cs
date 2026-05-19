namespace Encuesta.ModeloDatos;

public class OrganizacionRequest
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? UrlLogo { get; set; }
}
