namespace Encuesta.ModeloDatos;

public class Encuesta
{
    public Guid Id { get; set; }
    public Guid OrganizacionId { get; set; }
    public string Titulo { get; set; } = "";
    public string? Descripcion { get; set; }
    public int Version { get; set; }
    public string Estado { get; set; } = "";
    public bool EsGlobal { get; set; }
    public bool EsPlantilla { get; set; }
    public Guid? PlantillaOrigenId { get; set; }
    public string? EtiquetasJson { get; set; }
    public Guid CreadoPorUsuarioId { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public DateTime? PublicadoEn { get; set; }
    public string? ConfiguracionJson { get; set; }
    public DateTime CreadoEn { get; set; }
}
