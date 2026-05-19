namespace Encuesta.ModeloDatos;

public class Comentario
{
    public Guid Id { get; set; }
    public Guid? EntidadId { get; set; }
    public Guid? RespuestaId { get; set; }
    public Guid? UsuarioId { get; set; }
    public string TextoComentario { get; set; } = "";
    public DateTime CreadoEn { get; set; }
}
