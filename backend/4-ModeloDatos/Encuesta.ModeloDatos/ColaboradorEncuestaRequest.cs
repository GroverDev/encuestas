namespace Encuesta.ModeloDatos;

public class ColaboradorEncuestaRequest
{
    public Guid Id { get; set; }
    public Guid EncuestaId { get; set; }
    public Guid UsuarioId { get; set; }
    public string Rol { get; set; } = "EDITOR";
}
