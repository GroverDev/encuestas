namespace Encuesta.ModeloDatos;

public class Organizacion
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? UrlLogo { get; set; }
    public DateTime CreadoEn { get; set; }
}
