using Microsoft.Data.SqlClient;
using Npgsql;
using System.Data;

namespace Comun.BaseDatos
{
    public class Dao
    {
        #region atributos de la clase
        Conexion conexion;

        #region Atributos de SQL SERVER
        /// <summary>
        /// Variable de conexion
        /// </summary>
        private Microsoft.Data.SqlClient.SqlConnection CONEXION = null;
        /// <summary>
        /// Variable de Sentencia
        /// </summary>
        private Microsoft.Data.SqlClient.SqlCommand SENTENCIA;
        /// <summary>
        /// Variable de Transaccion
        /// </summary>
        private Microsoft.Data.SqlClient.SqlTransaction TRANSACCION;
        #endregion


        /// <summary>
        /// Gestor de la Base de Datos [SEL SERVER, POSTGRESQL]
        /// </summary>
        
        private string password = "";
        private string CadenaConexion;

        #endregion

        #region Instancia, Configuracion y Conexion de la BD

        public Dao(Enumeradores.Sistema sistema)
        {
            conexion = new Conexion(sistema);
            ConfigurarBD();
        }
        public Dao(Enumeradores.Sistema sistema, Enumeradores.BaseDeDatos basedatos)
        {
            conexion = new Conexion(sistema, basedatos);
            ConfigurarBD();
        }
        public Dao(Enumeradores.Sistema sistema, Enumeradores.BaseDeDatos basedatos, int IdRegional)
        {
            conexion = new Conexion(sistema, basedatos, IdRegional);
            ConfigurarBD();
        }
        public Dao(Enumeradores.Sistema sistema, Enumeradores.BaseDeDatos basedatos, int IdRegional, int gestion)
        {
            conexion = new Conexion(sistema, basedatos, IdRegional, gestion);
            
            ConfigurarBD();
        }

        /// <summary>
        /// Crea una instancia del acceso para conectarse aun detemrinado tag que se concatena con: serv; bbdd; user; y pass;
        /// </summary>

        private void ConfigurarBD()
        {

            try
            {
                this.CadenaConexion = conexion.CadenaConexion;
                this.password = conexion.Password;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar el archivo de configuracion de acceso a la base de datos ", ex);
            }
        }

        /// <summary>
        /// Método que realiza la conexión a la Base de Datos.
        /// </summary>
        public void Connect()
        {
            if (this.CONEXION != null && !this.CONEXION.State.Equals(ConnectionState.Closed))
            {
                throw new Exception("La conexion ya se encuentra abierta");
            }
            try
            {
                if (this.CONEXION == null)
                {
                    this.CONEXION = new SqlConnection();

                    Herramientas.Criptografia.Simetrica cri = new Herramientas.Criptografia.Simetrica(Herramientas.Criptografia.Simetrica.ServiceProviderEnum.TripleDES);
                    cri.Key = "@CSBP@2013_@";
                    cri.Salt = "@2013@CSBP_@";
                    string resultado = cri.DesEncriptar(this.password);
                    resultado = cri.DesEncriptar(resultado);
                    this.CONEXION.ConnectionString = CadenaConexion.Replace("CRYPTCSBP", resultado.Trim());
                    // this.CONEXION.ConnectionString = this.conexion.CadenaConexion;

                    // System.Security.SecureString pwd = new System.Security.SecureString();

                    //foreach (char c in cri.DesEncriptar(cri.DesEncriptar(this.conexion.Password))) { pwd.AppendChar(c); }
                    //pwd.MakeReadOnly();

                    //SqlCredential credencial = new SqlCredential(this.conexion.Usuario, pwd);
                    //this.CONEXION.Credential = credencial;

                }

                this.CONEXION.Open();

            }
            catch (DataException ex)
            {
                throw new Exception("Error en la Conexión", ex);
            }
        }

        /// <summary>
        /// Procedimiento que desconecta
        /// </summary>
        public void Disconnect()
        {
            if (this.CONEXION != null)
            {
                if (this.CONEXION.State.Equals(ConnectionState.Open))
                {
                    this.CONEXION.Close();
                }
            }
        }
        #endregion

        #region Manejo de transacciones
        /// <summary>
        /// Comienza una transacción en base a la conexion abierta.
        /// Todo lo que se ejecute luego de esta ionvocación estará 
        /// dentro de una tranasacción.
        /// </summary>

        public void BeginTransaccion()
        {
            if (this.TRANSACCION == null)
            {
                this.TRANSACCION = this.CONEXION.BeginTransaction();
            }
        }

        /// <summary>
        /// Cancela la ejecución de una transacción.
        /// Todo lo ejecutado entre ésta invocación y su 
        /// correspondiente <c>ComenzarTransaccion</c> será perdido.
        /// </summary>
        public void RollbackTransaccion()
        {
            if (this.TRANSACCION != null)
            {
                this.TRANSACCION.Rollback();
                this.TRANSACCION = null;
            }
        }

        /// <summary>
        /// Confirma todo los comandos ejecutados entre el <c>ComanzarTransaccion</c>
        /// y ésta invocación.
        /// </summary>
        public void CommitTransaccion()
        {
            if (this.TRANSACCION != null)
            {
                this.TRANSACCION.Commit();
            }
        }
        #endregion


        #region Crea la sentencia SQL
        /// <summary>
        /// Establece SqlCommand de tipo Procedimiento Almacenado.
        /// </summary>
        /// <param name="nameStoreProcedure">Nombre del Procedimiento Almacenado.</param>
        public void SetStoreProcedure(string nameStoreProcedure)
        {
            if (this.CONEXION == null)
            {
                throw new Exception("Necesita conectarse a la Base de Datos");
            }
            int tiempo = 25000;
            SENTENCIA = new SqlCommand(); ;
            SENTENCIA.Connection = this.CONEXION;
            SENTENCIA.CommandTimeout = tiempo;
            SENTENCIA.CommandType = System.Data.CommandType.StoredProcedure;
            SENTENCIA.CommandText = nameStoreProcedure;
            if (this.TRANSACCION != null)
            {
                this.SENTENCIA.Transaction = this.TRANSACCION;
            }
                    
        }

        /// <summary>
        /// Estabelce la sentencia o query SqlCommand.
        /// </summary>
        /// <param name="querySQL">Sentencia sql.</param>
        public void SetQuerySQL(string querySQL)
        {
            this.SENTENCIA = new SqlCommand();
            this.SENTENCIA.Connection = this.CONEXION;
            this.SENTENCIA.CommandType = CommandType.Text;
            this.SENTENCIA.CommandTimeout = 55;
            this.SENTENCIA.CommandText = querySQL;

            if (this.TRANSACCION != null)
            {
                this.SENTENCIA.Transaction = this.TRANSACCION;
            }
        }

        /// <summary>
        /// Establece la sentencia o query SqlCommand y el tiempo de espera.
        /// </summary>
        /// <param name="querySQL">Sentencia sql.</param>
        public void SetQuerySQLTimeout(string querySQL, int timeOut)
        {
            this.SENTENCIA = new SqlCommand();
            this.SENTENCIA.Connection = this.CONEXION;
            this.SENTENCIA.CommandType = CommandType.Text;
            this.SENTENCIA.CommandTimeout = timeOut;
            this.SENTENCIA.CommandText = querySQL;

            if (this.TRANSACCION != null)
            {
                this.SENTENCIA.Transaction = this.TRANSACCION;
            }
        }


        /// <summary>
        /// Asigna el valor de parametro al prametro del store procedure
        /// </summary>
        /// <param name="parametro">paramtero del Store Procedure.</param>
        /// <param name="parametroValor">Valor del Paramtero.</param>
        /// <param name="tipo">Tipo de datos SqlServer.</param>
        private NpgsqlParameter AsignaValorParametropg(NpgsqlParameter parametro, object parametroValor, Parameter.DataType tipo)
        {

            if ((parametro != null) && (parametroValor != null))
            {
                switch (tipo)
                {
                    case Parameter.DataType.Integer:
                        parametro.Value = Convert.ToInt32(parametroValor);
                        break;
                    case Parameter.DataType.LongInt:
                        parametro.Value = Convert.ToInt64(parametroValor);
                        break;
                    case Parameter.DataType.ShortInt:
                        parametro.Value = Convert.ToInt16(parametroValor);
                        break;
                    case Parameter.DataType.Decimal:
                        parametro.Value = Convert.ToDecimal(parametroValor);
                        break;
                    case Parameter.DataType.Float:
                        parametro.Value = Convert.ToInt16(parametroValor);
                        break;
                    case Parameter.DataType.Currency:
                        parametro.Value = Convert.ToInt16(parametroValor);
                        break;
                    case Parameter.DataType.Bit:
                        parametro.Value = Convert.ToBoolean(parametroValor);
                        break;
                    case Parameter.DataType.VarChar:
                        parametro.Value = Convert.ToString(parametroValor);
                        break;
                    case Parameter.DataType.VarChar2:
                        parametro.Value = Convert.ToInt16(parametroValor);
                        break;
                    case Parameter.DataType.Date:
                        parametro.Value = Convert.ToDateTime(parametroValor);
                        break;
                    case Parameter.DataType.DateTime:
                        parametro.Value = Convert.ToDateTime(parametroValor);
                        break;
                    case Parameter.DataType.VarBinary:
                        parametro.Value = Convert.ToInt16(parametroValor);
                        break;
                    case Parameter.DataType.UniqueIdentifier:
                        parametro.Value = (Guid)(parametroValor);
                        break;
                    case Parameter.DataType.Boolean:
                        parametro.Value = Convert.ToBoolean(parametroValor);
                        break;
                    case Parameter.DataType.Memo:
                        parametro.Value = Convert.ToInt16(parametroValor);
                        break;
                    default:
                        break;
                }
            }

            return parametro;
        }


        private SqlParameter AsignaValorParametro(SqlParameter parametro, object parametroValor, Parameter.DataType tipo)
        {

            if ((parametro != null) && (parametroValor != null))
            {
                switch (tipo)
                {
                    case Parameter.DataType.Integer:
                        parametro.Value = Convert.ToInt32(parametroValor);
                        break;
                    case Parameter.DataType.LongInt:
                        parametro.Value = Convert.ToInt64(parametroValor);
                        break;
                    case Parameter.DataType.ShortInt:
                        parametro.Value = Convert.ToInt16(parametroValor);
                        break;
                    case Parameter.DataType.Decimal:
                        parametro.Value = Convert.ToDecimal(parametroValor);
                        break;
                    case Parameter.DataType.Float:
                        parametro.Value = Convert.ToDouble(parametroValor);
                        break;
                    case Parameter.DataType.Currency:
                        parametro.Value = Convert.ToDecimal(parametroValor);
                        break;
                    case Parameter.DataType.Bit:
                        parametro.Value = Convert.ToBoolean(parametroValor);
                        break;
                    case Parameter.DataType.VarChar:
                        parametro.Value = Convert.ToString(parametroValor);
                        break;
                    case Parameter.DataType.VarChar2:
                        parametro.Value = Convert.ToString(parametroValor);
                        break;
                    case Parameter.DataType.Date:
                        parametro.Value = Convert.ToDateTime(parametroValor);
                        break;
                    case Parameter.DataType.DateTime:
                        parametro.Value = Convert.ToDateTime(parametroValor);
                        break;
                    case Parameter.DataType.VarBinary:
                        byte[] data= (byte[])parametroValor;
                        parametro.Size = data.Length;
                        parametro.Value = data;
                        break;
                    case Parameter.DataType.UniqueIdentifier:
                        parametro.Value = (Guid)(parametroValor);
                        break;
                    case Parameter.DataType.Boolean:
                        parametro.Value = Convert.ToBoolean(parametroValor);
                        break;
                    /*case Parameter.DataType.Memo:
                        parametro.Value = Convert.ToInt16(parametroValor);
                        break;*/
                    default:
                        break;
                }
            }

            return parametro;
        }
        /// <summary>
        /// Adiciona el parametro al store procedure o texto en SQL
        /// </summary>
        /// <param name="nameParameter">Nombre del Parametro ejm @valor</param>
        /// <param name="dataType">Tipo de dato de SqlServer.</param>
        /// <param name="value">Valor del Parametro</param>
        public void AddParameter(string nameParameter, Parameter.DataType dataType, object value)
        {
            //recupera el parametro del store procedure
            SqlParameter PARAMETRO = new SqlParameter(nameParameter, dataType)
            {
                Direction = ParameterDirection.Input
            };
            //PARAMETRO.Value = valorChar;


            //asigan el valor al parametro
            AsignaValorParametro(PARAMETRO, value, dataType);

            if ((PARAMETRO.Direction == ParameterDirection.InputOutput) && (PARAMETRO.Value == null))
            {
                PARAMETRO.Value = DBNull.Value;
            }

            SENTENCIA.Parameters.Add(PARAMETRO);
            
        }
        public void AddParameterNULL(string nameParameter)
        {
           
            SqlDbType tipo = SqlDbType.VarChar;
            //recupera el parametro del store procedure
            SqlParameter PARAMETRO = new SqlParameter(nameParameter, tipo);
            PARAMETRO.Direction = ParameterDirection.Input;
            //PARAMETRO.Value = valorChar;

            if ((PARAMETRO.Direction == ParameterDirection.Input) && (PARAMETRO.Value == null))
            {
                PARAMETRO.Value = DBNull.Value;
            }

            SENTENCIA.Parameters.Add(PARAMETRO);
        }


        #endregion
        #region transacciones con sentencia
        /// <summary>
        /// Ejecuta la sentencia
        /// </summary>
        /// <returns></returns>
        public void ExecuteQuery()
        {
            try
            {
                this.SENTENCIA.ExecuteNonQuery();
                this.SENTENCIA.Parameters.Clear();
            }
            catch (DataException ex)
            {
                throw new Exception("Error en la Ejecución de la Sentencia", ex);
                //throw new BaseDeDatosException("Error en la Ejecución de la Sentencia", ex);
            }
        }
        /// <summary>
        /// Ejecuta la sentencia 
        /// </summary>
        /// <returns>Entero: Numero de filas afectadas</returns>
        public int ExecuteNonQuery()
        {
            int filas;
            try
            {
                filas = this.SENTENCIA.ExecuteNonQuery();
                this.SENTENCIA.Parameters.Clear();
            }
            catch (DataException ex)
            {
                throw new Exception("Error en la Ejecución del NonQuery", ex);
            }
            return filas;
        }
        /// <summary>
        /// Ejecuta la sentencia o comando (retorna el resultado en un DataSet )
        /// </summary>
        /// <returns></returns>
        public DataSet ExecuteDataSet()
        {
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter();
            try
            {
                da = new SqlDataAdapter(this.SENTENCIA);
                da.Fill(ds);
                this.SENTENCIA.Parameters.Clear();

            }
            catch (DataException ex)
            {
                throw new Exception("No hay Datos", ex);
            }

            finally
            {
                da.Dispose();
            }
            return ds;
        }
        /// <summary>
        /// Ejecuta la sentencia o comando (retorna el resultado en un DataTable )
        /// </summary>
        /// <returns></returns>
        public DataTable ExecuteDataTable()
        {
            DataTable dt = new DataTable();
           
            SqlDataAdapter da = new SqlDataAdapter();
            try
            {
                da = new SqlDataAdapter(this.SENTENCIA);
                da.Fill(dt);
                this.SENTENCIA.Parameters.Clear();

            }
            catch (DataException ex)
            {
                throw new Exception(ex.Message);
            }

            finally
            {
                da.Dispose();
            }
            return dt;

        }
        /// <summary>
        /// Ejecuta la sentencia o comando (retorna el resultado en un DataSet) con un nombre de tabla
        /// </summary>
        /// <returns></returns>
        public DataSet ExecuteDataSet(string nombreTabla)
        {
            DataSet ds = new DataSet();
            
            try
            {
                SqlDataAdapter da = new SqlDataAdapter(this.SENTENCIA);
                da.Fill(ds, nombreTabla);
                this.SENTENCIA.Parameters.Clear();

            }
            catch (DataException ex)
            {
                throw new Exception("No hay Datos", ex);
            }
            return ds;

        }

        /// <summary>
        /// Ejecuta la sentencia o comando (retorna el resultado en un SQLDataReader )
        /// </summary>
        /// <returns></returns>
        public SqlDataReader ExecuteReader()
        {
            SqlDataReader dr;
            try
            {
                dr = this.SENTENCIA.ExecuteReader();
                this.SENTENCIA.Parameters.Clear();
            }
            catch (DataException ex)
            {
                throw new Exception("Error en la Ejecución de DataReader", ex);
            }

            return dr;


        }
        /// <summary>
        /// Ejecuta la sentencia o comando (retorna el resultado 1x1 )
        /// </summary>
        /// <returns></returns>
        public object ExecuteScalar()
        {
            object valor;
            try
            {
                valor = this.SENTENCIA.ExecuteScalar();
                this.SENTENCIA.Parameters.Clear();
            }
            catch (DataException ex)
            {
                throw new Exception("Error en la Ejecución de DataReader", ex);
            }
            return valor;
        }
        #endregion

        
    }
}