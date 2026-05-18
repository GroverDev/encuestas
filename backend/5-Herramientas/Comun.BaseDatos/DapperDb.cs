using Comun.Herramientas;
using Npgsql;
using System.Data;
using Microsoft.Data.SqlClient;
using static Comun.BaseDatos.Enumeradores;

namespace Comun.BaseDatos;

public class DapperDb
{
    #region Atributos de Sql Server
    private string password;
    private string cadenaConexion;
    public IDbConnection CreateConnection;
    #endregion

    #region Instancia, Configuracion y Conexion de la BD

    public DapperDb(GestorDB gestor, Sistema sistema)
    {
        string connectionStringGestorDB = gestor == GestorDB.SQLSERVER ? "ConnectionStrings" : "ConnectionStringsPg";
        ArmaCadenaConexion(Appsettings.GetValor(connectionStringGestorDB, Enum.GetName(typeof(Sistema), sistema)), gestor);
        CreateConnection = gestor == GestorDB.SQLSERVER 
                           ? new SqlConnection(descifrarContrasenia())
                           : new NpgsqlConnection(descifrarContrasenia());
    }

    public DapperDb(GestorDB gestor, Sistema sistema, BaseDeDatos basedatos)
    {
        string connectionStringGestorDB = gestor == GestorDB.SQLSERVER ? "ConnectionStrings" : "ConnectionStringsPg";
        string connectionStringTag = Enum.GetName(typeof(Sistema), sistema) + Enum.GetName(typeof(BaseDeDatos), basedatos);
        ArmaCadenaConexion(Appsettings.GetValor(connectionStringGestorDB, connectionStringTag), gestor);
        CreateConnection = gestor == GestorDB.SQLSERVER
                           ? new SqlConnection(descifrarContrasenia())
                           : new NpgsqlConnection(descifrarContrasenia());
    }
    public DapperDb(GestorDB gestor, Sistema sistema, BaseDeDatos basedatos, int IdRegional)
    {
        string connectionStringGestorDB = gestor == GestorDB.SQLSERVER ? "ConnectionStrings" : "ConnectionStringsPg";
        string regional = Enum.GetName(typeof(Regional), IdRegional);
        string connectionStringTag = Enum.GetName(typeof(Sistema), sistema) + Enum.GetName(typeof(BaseDeDatos), basedatos) + regional;
        ArmaCadenaConexion(Appsettings.GetValor(connectionStringGestorDB, connectionStringTag), gestor);
        CreateConnection = gestor == GestorDB.SQLSERVER
                           ? new SqlConnection(descifrarContrasenia())
                           : new NpgsqlConnection(descifrarContrasenia());
    }
    public DapperDb(GestorDB gestor, Sistema sistema, BaseDeDatos basedatos, int IdRegional, int gestion)
    {
        string connectionStringGestorDB = gestor == GestorDB.SQLSERVER ? "ConnectionStrings" : "ConnectionStringsPg";

        string regional = Enum.GetName(typeof(Regional), IdRegional);
        string connectionStringTag = Enum.GetName(typeof(Sistema), sistema) + Enum.GetName(typeof(BaseDeDatos), basedatos) + regional + gestion;
        ArmaCadenaConexion(Appsettings.GetValor(connectionStringGestorDB, connectionStringTag), gestor);
        CreateConnection = gestor == GestorDB.SQLSERVER
                           ? new SqlConnection(descifrarContrasenia())
                           : new NpgsqlConnection(descifrarContrasenia());
    }


    private void ArmaCadenaConexion(string cadena, GestorDB gestor)
    {
        char delimiter = ';';
        String[] valores = cadena.Split(delimiter);

        string servidor = valores[0].ToString().Split("=")[1].TrimStart().TrimEnd();
        string basedatos = valores[1].ToString().Split("=")[1].TrimStart().TrimEnd();
        string usuario = valores[2].ToString().Split("=")[1].TrimStart().TrimEnd();
        string password = valores[3].Replace("Password=", "").Replace("Password =", "").Replace("PASSWORD=", "").Replace("PASSWORD =", "");

        this.password = password.Trim();
        if (gestor == GestorDB.SQLSERVER)
            this.cadenaConexion = "Server = " + servidor + "; Database = " + basedatos + "; User Id = " + usuario + "; Password = CRYPTCSBP; TrustServerCertificate=true;";
        else
        {
            string puerto = (valores.Length >= 4) ? valores[4].ToString().Split("=")[1].TrimStart().TrimEnd() : "5432";
            this.cadenaConexion = "Server = " + servidor + "; Database = " + basedatos + "; User Id = " + usuario + "; Password = CRYPTCSBP; Port=" + puerto + ";";
        }
    }

    private string descifrarContrasenia()
    {
        Herramientas.Criptografia.Simetrica cri = new Herramientas.Criptografia.Simetrica(Herramientas.Criptografia.Simetrica.ServiceProviderEnum.TripleDES);
        cri.Key = "@CSBP@2013_@";
        cri.Salt = "@2013@CSBP_@";
        string resultado = cri.DesEncriptar(this.password);
        resultado = cri.DesEncriptar(resultado);
        this.cadenaConexion = this.cadenaConexion.Replace("CRYPTCSBP", resultado.Trim());

        return this.cadenaConexion;
    }
    #endregion
}
