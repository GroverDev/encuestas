namespace Encuesta.ModeloDatos;

public class LoginResponse
{
    public string Token { get; set; } = "";
    public CuentaUsuarioResponse Usuario { get; set; } = new();
}
