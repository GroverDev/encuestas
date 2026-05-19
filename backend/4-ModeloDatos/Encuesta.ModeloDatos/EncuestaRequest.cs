namespace Encuesta.ModeloDatos;

public class EncuestaRequest
{
    public Guid Id { get; set; }
    public Guid OrganizacionId { get; set; }
    public Guid CreadoPorUsuarioId { get; set; }
    public string Titulo { get; set; } = "";
    public string? Descripcion { get; set; }
    public bool EsGlobal { get; set; }
    public bool EsPlantilla { get; set; }
    public Guid? PlantillaOrigenId { get; set; }
    public string? EtiquetasJson { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? ConfiguracionJson { get; set; }
}
