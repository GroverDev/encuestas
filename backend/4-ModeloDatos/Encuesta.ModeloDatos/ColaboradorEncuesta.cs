namespace Encuesta.ModeloDatos;

public class ColaboradorEncuesta
{
    public Guid Id { get; set; }
    public Guid EncuestaId { get; set; }
    public Guid UsuarioId { get; set; }
    public string Rol { get; set; } = "";
    public DateTime InvitadoEn { get; set; }
}
