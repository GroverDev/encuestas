using Microsoft.Data.SqlClient;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace Comun.BaseDatos
{
    public class DaoPg
    {
        #region atributos de la clase
        ConexionPg conexion;

        #region Atributos de PostgreSQL
        /// <summary>
        /// Variable de conexion
        /// </summary>
        private NpgsqlConnection CONEXIONpg = null;
        /// <summary>
        /// Variable de Sentencia
        /// </summary>
        private NpgsqlCommand SENTENCIApg;
        /// <summary>
        /// Variable de Transaccion
        /// </summary>
        private NpgsqlTransaction TRANSACCIONpg;


        #endregion
        
        /// <summary>
        /// Gestor de la Base de Datos [SEL SERVER, POSTGRESQL]
        /// </summary>
        
        private string password = "";
        private string CadenaConexion;

        #endregion

        #region Instancia, Configuracion y Conexion de la BD

        public DaoPg(Enumeradores.Sistema sistema)
        {
            conexion = new ConexionPg(sistema);
            ConfigurarBD();
        }
        public DaoPg(Enumeradores.Sistema sistema, Enumeradores.BaseDeDatos basedatos)
        {
            conexion = new ConexionPg(sistema, basedatos);
            ConfigurarBD();
        }
        public DaoPg(Enumeradores.Sistema sistema, Enumeradores.BaseDeDatos basedatos, int IdRegional)
        {
            conexion = new ConexionPg(sistema, basedatos, IdRegional);
            ConfigurarBD();
        }
        public DaoPg(Enumeradores.Sistema sistema, Enumeradores.BaseDeDatos basedatos, int IdRegional, int gestion)
        {
            conexion = new ConexionPg(sistema, basedatos, IdRegional, gestion);
            
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
            if (this.CONEXIONpg != null && !this.CONEXIONpg.State.Equals(ConnectionState.Closed))
            {
                //throw new BaseDeDatosException("La conexion ya se encuentra abierta");
                throw new Exception("Error");
            }
            try
            {
                if (this.CONEXIONpg == null)
                {
                    this.CONEXIONpg = new NpgsqlConnection();
                    Herramientas.Criptografia.Simetrica cri = new Herramientas.Criptografia.Simetrica(Herramientas.Criptografia.Simetrica.ServiceProviderEnum.TripleDES);
                    cri.Key = "@CSBP@2013_@";
                    cri.Salt = "@2013@CSBP_@";
                    string resultado = cri.DesEncriptar(this.password);
                    resultado = cri.DesEncriptar(resultado);
                    this.CONEXIONpg.ConnectionString = CadenaConexion.Replace("CRYPTCSBP", resultado.Trim());
                }

                this.CONEXIONpg.Open();

            }
            catch (DataException ex)
            {
                //throw new BaseDeDatosException("Error en la Conexión", ex);
                throw new Exception(ex.Message);
            }
                
            


        }

        /// <summary>
        /// Procedimiento que desconecta
        /// </summary>
        public void Disconnect()
        {
            if (this.CONEXIONpg != null)
            {
                if (this.CONEXIONpg.State.Equals(ConnectionState.Open))
                {
                    this.CONEXIONpg.Close();
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
            if (this.TRANSACCIONpg == null)
            {
                this.TRANSACCIONpg = this.CONEXIONpg.BeginTransaction();
            }
        }

        /// <summary>
        /// Cancela la ejecución de una transacción.
        /// Todo lo ejecutado entre ésta invocación y su 
        /// correspondiente <c>ComenzarTransaccion</c> será perdido.
        /// </summary>
        public void RollbackTransaccion()
        {
            if (this.TRANSACCIONpg != null)
            {
                this.TRANSACCIONpg.Rollback();
                this.TRANSACCIONpg = null;
            }
        }

        /// <summary>
        /// Confirma todo los comandos ejecutados entre el <c>ComanzarTransaccion</c>
        /// y ésta invocación.
        /// </summary>
        public void CommitTransaccion()
        {
            if (this.TRANSACCIONpg != null)
            {
                this.TRANSACCIONpg.Commit();
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

            if (this.CONEXIONpg == null)
            {
                //throw new BaseDeDatosException("Necesita conectarse a la Base de Datos");
                throw new Exception("Necesita conectarse a la Base de Datos");
            }
            int tiempopg = 25000;
            SENTENCIApg = new NpgsqlCommand(); ;
            SENTENCIApg.Connection = this.CONEXIONpg;
            SENTENCIApg.CommandTimeout = tiempopg;
            SENTENCIApg.CommandType = System.Data.CommandType.StoredProcedure;
            SENTENCIApg.CommandText = nameStoreProcedure;
            if (this.TRANSACCIONpg != null)
            {
                this.SENTENCIApg.Transaction = this.TRANSACCIONpg;
            }
                   
        }

        /// <summary>
        /// Estabelce la sentencia o query SqlCommand.
        /// </summary>
        /// <param name="querySQL">Sentencia sql.</param>
        public void SetQuerySQL(string querySQL)
        {
            this.SENTENCIApg = new NpgsqlCommand();
            this.SENTENCIApg.Connection = this.CONEXIONpg;
            this.SENTENCIApg.CommandType = CommandType.Text;
            this.SENTENCIApg.CommandText = querySQL;

            if (this.TRANSACCIONpg != null)
            {
                this.SENTENCIApg.Transaction = this.TRANSACCIONpg;
            }
        }

        /// <summary>
        /// Establece la sentencia o query SqlCommand y el tiempo de espera.
        /// </summary>
        /// <param name="querySQL">Sentencia sql.</param>
        public void SetQuerySQLTimeout(string querySQL, int timeOut)
        {
            this.SENTENCIApg = new NpgsqlCommand();
            this.SENTENCIApg.Connection = this.CONEXIONpg;
            this.SENTENCIApg.CommandType = CommandType.Text;
            this.SENTENCIApg.CommandText = querySQL;
            this.SENTENCIApg.CommandTimeout = timeOut;

            if (this.TRANSACCIONpg != null)
            {
                this.SENTENCIApg.Transaction = this.TRANSACCIONpg;
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
        private NpgsqlParameter AsignaValorParametro2pg(NpgsqlParameter parametro, object parametroValor, NpgsqlDbType tipo)
        {
            
            if ((parametro != null) && (parametroValor != null))
            {
                switch (tipo)
                {
                    case NpgsqlDbType.Json:
                        parametro.Value = Convert.ToString(parametroValor);
                        break;
                    case NpgsqlDbType.Boolean:
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
                        parametro.Value = Convert.ToInt16(parametroValor);
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
            NpgsqlParameter PARAMETROpg = new NpgsqlParameter(nameParameter, dataType);
            PARAMETROpg.Direction = ParameterDirection.Input;
            //PARAMETRO.Value = valorChar;


            //asigan el valor al parametro
            AsignaValorParametropg(PARAMETROpg, value, dataType);

            if ((PARAMETROpg.Direction == ParameterDirection.InputOutput) && (PARAMETROpg.Value == null))
            {
                PARAMETROpg.Value = DBNull.Value;
            }

            SENTENCIApg.Parameters.Add(PARAMETROpg);
        }
        public void AddParameterPg(string nameParameter, NpgsqlDbType dataType, object value)
        {
            //recupera el parametro del store procedure
            NpgsqlParameter PARAMETROpg = new NpgsqlParameter(nameParameter, dataType);
            PARAMETROpg.Direction = ParameterDirection.Input;
         


            //asigan el valor al parametro
            AsignaValorParametro2pg(PARAMETROpg, value, dataType);

            if ((PARAMETROpg.Direction == ParameterDirection.InputOutput) && (PARAMETROpg.Value == null))
            {
                PARAMETROpg.Value = DBNull.Value;
            }

            SENTENCIApg.Parameters.Add(PARAMETROpg);
        }
        //public void AddParameterNULL(string nameParameter)
        //{
        //    NpgsqlDbType tipopg = NpgsqlDbType.;
        //    //recupera el parametro del store procedure
        //    NpgsqlParameter PARAMETROpg = new NpgsqlParameter(nameParameter, tipopg);
        //    PARAMETROpg.Direction = ParameterDirection.Input;
        //    //PARAMETRO.Value = valorChar;

        //    if ((PARAMETROpg.Direction == ParameterDirection.Input) && (PARAMETROpg.Value == null))
        //    {
        //        PARAMETROpg.Value = DBNull.Value;
        //    }

        //    SENTENCIApg.Parameters.Add(PARAMETROpg);

        //}


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
                this.SENTENCIApg.ExecuteNonQuery();
                this.SENTENCIApg.Parameters.Clear();
            }
            catch (DataException ex)
            {
                throw new Exception("Error en la Ejecución de la Sentencia", ex);
                //throw new BaseDeDatosException("Error en la Ejecución de la Sentencia", ex);
            }
            catch (Npgsql.NpgsqlException e)
            {
                string error = "";
                throw new Exception(e.Message);

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
                filas = this.SENTENCIApg.ExecuteNonQuery();
                this.SENTENCIApg.Parameters.Clear();
            }
            catch (DataException ex)
            {
                throw new Exception("Error en la Ejecución del NonQuery", ex);
            }
            catch (NpgsqlException e)
            {
                throw new NpgsqlException("Error en la Ejecución del NonQuery", e);
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
            NpgsqlDataAdapter dapg = new NpgsqlDataAdapter();
            try
            {
                dapg = new NpgsqlDataAdapter(this.SENTENCIApg);
                dapg.Fill(ds);
                this.SENTENCIApg.Parameters.Clear();

            }
            catch (DataException ex)
            {
                throw new Exception("No hay Datos", ex);
            }
            catch (Npgsql.NpgsqlException e)
            {
                string error = "";
                throw new Exception(error);
            }
            finally
            {
                dapg.Dispose();
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
            NpgsqlDataAdapter dapg = new NpgsqlDataAdapter();
            try
            {
                dapg = new NpgsqlDataAdapter(this.SENTENCIApg);
                dapg.Fill(dt);
                this.SENTENCIApg.Parameters.Clear();

            }
            catch (DataException ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                dapg.Dispose();
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
                NpgsqlDataAdapter dapg = new NpgsqlDataAdapter(this.SENTENCIApg);
                dapg.Fill(ds, nombreTabla);
                this.SENTENCIApg.Parameters.Clear();

            }
            catch (DataException ex)
            {
                throw new Exception("No hay Datos", ex);
            }
            return ds;
        }
        /// <summary>
        /// Ejecuta la sentencia o comando (retorna el resultado en un NpgsqlDataReader )
        /// </summary>
        /// <returns></returns>
        public NpgsqlDataReader ExecuteReader()
        {
            NpgsqlDataReader dr;
            try
            {
                dr = this.SENTENCIApg.ExecuteReader();
                this.SENTENCIApg.Parameters.Clear();
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
                valor = this.SENTENCIApg.ExecuteScalar();
                this.SENTENCIApg.Parameters.Clear();
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