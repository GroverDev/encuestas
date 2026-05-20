using Comun.Herramientas;
using Comun.Herramientas.Criptografia;
using Encuesta.AccesoDatos;
using Encuesta.ModeloDatos;

namespace Encuesta.LogicaNegocios;

public static class AuthBLL
{
    public static async Task<Respuesta<CuentaUsuario>> ValidarCredenciales(LoginRequest request)
    {
        var response = new Respuesta<CuentaUsuario>();
        try
        {
            var usuario = await CuentaUsuarioDAL.ObtenerCuentaUsuarioPorCorreo(request.Correo);

            if (usuario is null || !ASimetrica.VerifyPassword(usuario.HashContrasena, request.Contrasena))
                throw new ExceptionControlado("Correo o contraseña incorrectos.");

            response.Datos = usuario;
            response.ok = true;
        }
        catch (ExceptionControlado ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        catch (Exception ex) { response.SetMensaje(TiposMensaje.Error, "Atención", ex.Message); }
        return response;
    }
}
