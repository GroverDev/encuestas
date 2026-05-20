namespace Encuesta.ModeloDatos;

public class EncuestaPublicaResponse
{
    public Guid InvitacionId { get; set; }
    public Guid EncuestaId { get; set; }
    public string Titulo { get; set; } = "";
    public string? Descripcion { get; set; }
    public int Version { get; set; }
    public List<SeccionPublicaResponse> Secciones { get; set; } = new();
    public List<PreguntaPublicaResponse> Preguntas { get; set; } = new();
    public List<string> Reglas { get; set; } = new();
}

public class SeccionPublicaResponse
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = "";
    public string? Descripcion { get; set; }
    public int Orden { get; set; }
}

public class PreguntaPublicaResponse
{
    public Guid Id { get; set; }
    public Guid? SeccionId { get; set; }
    public string Tipo { get; set; } = "";
    public string Titulo { get; set; } = "";
    public string? Descripcion { get; set; }
    public int Orden { get; set; }
    public bool EsObligatoria { get; set; }
    public string? ConfiguracionJson { get; set; }
    public List<OpcionPreguntaResponse> Opciones { get; set; } = new();
}
