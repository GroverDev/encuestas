using Microsoft.Extensions.Configuration;

namespace Comun.BaseDatos;

public class Conexion
{
    private string servidor;

    public string Servidor
    {
        get { return servidor; }
        set { servidor = value; }
    }
    private string basedatos;

    public string BaseDatos
    {
        get { return basedatos; }
        set { basedatos = value; }
    }

    private string usuario;

    public string Usuario
    {
        get { return usuario; }
        set { usuario = value; }
    }
    private string password;

    public string Password
    {
        get { return password; }
        set { password = value; }
    }

    private string cadenaConexion;
    public string CadenaConexion
    {
        get { return cadenaConexion; }
        set { cadenaConexion = value; }
    }

    public Conexion(Enumeradores.Sistema sistema)
    {
        string cadenaConexion = getConectionString(Enum.GetName(typeof(Enumeradores.Sistema), sistema));
        ArmaCadenaConexion(cadenaConexion);
    }
    public Conexion(Enumeradores.Sistema sistema, Enumeradores.BaseDeDatos basedatos)
    {
        string cadenaConexion = getConectionString(Enum.GetName(typeof(Enumeradores.Sistema), sistema) + Enum.GetName(typeof(Enumeradores.BaseDeDatos), basedatos));
        ArmaCadenaConexion(cadenaConexion);
    }
    public Conexion(Enumeradores.Sistema sistema, Enumeradores.BaseDeDatos basedatos, int IdRegional)
    {
        string regional = Enum.GetName(typeof(Enumeradores.Regional), IdRegional);
        string cadenaConexion = getConectionString(Enum.GetName(typeof(Enumeradores.Sistema), sistema) + Enum.GetName(typeof(Enumeradores.BaseDeDatos), basedatos) + regional);
        ArmaCadenaConexion(cadenaConexion);
    }
    public Conexion(Enumeradores.Sistema sistema, Enumeradores.BaseDeDatos basedatos, int IdRegional, int gestion)
    {
        string regional = Enum.GetName(typeof(Enumeradores.Regional), IdRegional);
        string cadenaConexion = getConectionString(Enum.GetName(typeof(Enumeradores.Sistema), sistema) + Enum.GetName(typeof(Enumeradores.BaseDeDatos), basedatos) + regional + gestion.ToString());
        ArmaCadenaConexion(cadenaConexion);
    }

    private void ArmaCadenaConexion(string cadena)
    {
        char delimiter = ';';
        String[] valores = cadena.Split(delimiter);

        string servidor = valores[0].ToString().Split("=")[1].TrimStart().TrimEnd();
        string basedatos = valores[1].ToString().Split("=")[1].TrimStart().TrimEnd();
        string usuario = valores[2].ToString().Split("=")[1].TrimStart().TrimEnd();

        string password = valores[3].Replace("Password=", "").Replace("Password =", "").Replace("PASSWORD=", "").Replace("PASSWORD =", "");
        this.basedatos = basedatos;
        this.servidor = servidor;
        this.usuario = usuario;
        this.password = password.Trim();
        
        this.cadenaConexion = "Server = " + servidor + "; Database = " + basedatos + "; User Id = " + usuario+ "; Password = CRYPTCSBP; TrustServerCertificate=true;";
    }



    private static string getConectionString(string cadena)
    {
        try
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var config = builder.Build();
            var appConfig = config.GetSection("ConnectionStrings").Exists()
               ? (config.GetSection("ConnectionStrings").GetSection(cadena).Exists() ? config.GetSection("ConnectionStrings").GetSection(cadena).Value : "No existe el  tag") : "No existe el  tag";
            
            if (appConfig == "No existe el  tag")
                throw new Exception("No existe el  tag: ConnectionStrings y/o " + cadena + "  en el appsettings.");

            return appConfig.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }


}
