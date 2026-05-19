namespace Encuesta.ModeloDatos;

public class EntidadRequest
{
    public Guid Id { get; set; }
    public Guid OrganizacionId { get; set; }
    public Guid TipoEntidadId { get; set; }
    public Guid? EntidadPadreId { get; set; }
    public string NombreVisible { get; set; } = "";
    public string? IdExterno { get; set; }
    public string? AtributosJson { get; set; }
}
