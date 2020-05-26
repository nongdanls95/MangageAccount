using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using WebApp.Classes;

/// <summary>
/// Summary description for Connector
/// </summary>
namespace WebApp.Classes.Connector
{
    public enum ProviderFactory
    {
        MSSQL = 0,
        Oracle = 1,
        MySQL = 2,
        PostgreSQL = 3
    }
    public interface IDatabaseConnector
    {
        string ConnectionString
        {
            get;
            set;
        }
        ProviderFactory Provider
        {
            get;
            set;
        }
        string AssemblyVersion
        {
            get;
            set;
        }
        System.Globalization.CultureInfo CultureInfo
        {
            get;
            set;
        }
        string DateTimeFormat
        {
            get;
            set;
        }
        double TimeZoneOffset
        {
            get;
            set;
        }

        QueryResult ExecuteStoredProcedure(Stream requestStream);
        QueryResult ExecuteStoredProcedure(QueryParameter queryParam);
        QueryResult ExecuteStoredProcedure(string storeName);
        QueryResult ExecuteStoredProcedure(string storeName, Stream requestStream);
        QueryResult ExecuteStoredProcedure(string storeName, QueryParameter queryParam);

        QueryResult ExecuteStatement(string statement);
        QueryResult ExecuteStatement(string statement, QueryParameter queryParam);

    }

    public class OracleConnectorClass : DatabaseConnectorClass
    {
        public OracleConnectorClass()
            : base(ProviderFactory.Oracle)
        {
        }
        public OracleConnectorClass(string connectionString)
            : base(connectionString, ProviderFactory.Oracle)
        {
        }
    }
    public class SqlConnectorClass : DatabaseConnectorClass
    {
        public SqlConnectorClass()
            : base(ProviderFactory.MSSQL)
        {
        }
        public SqlConnectorClass(string connectionString)
            : base(connectionString, ProviderFactory.MSSQL)
        {
        }
    }
    public class MySqlConnectorClass : DatabaseConnectorClass
    {
        public MySqlConnectorClass()
            : base(ProviderFactory.MySQL)
        {
        }
        public MySqlConnectorClass(string connectionString)
            : base(connectionString, ProviderFactory.MySQL)
        {
        }
    }
    public class PostgreSqlConnectorClass : DatabaseConnectorClass
    {
        public PostgreSqlConnectorClass()
            : base(ProviderFactory.PostgreSQL)
        {
        }
        public PostgreSqlConnectorClass(string connectionString)
            : base(connectionString, ProviderFactory.PostgreSQL)
        {
        }
    }

    public class DatabaseConnectorClass : IDatabaseConnector
    {
        protected static string _OracleAssembly = "Oracle.DataAccess";
        protected static string _MySqlAssembly = "MySql.Data";
        protected static string _PostgreSqlAssembly = "Npgsql";

        protected IDbConnection _dbConnection = null;
        protected object _dbCoreConnection = null;

        protected string _assemblyName;
        protected string _connectionTypeName;
        protected string _commandTypeName;
        protected string _commandBuilderTypeName;
        protected string _parameterTypeName;
        protected string _paramNamePrefix;

        protected static ConcurrentDictionary<string, Assembly> _dictAssemblyCache = new ConcurrentDictionary<string, Assembly>();

        protected Type _connectionType;
        protected Type _commandType;
        protected Type _commandBuilderType;
        protected Type _parameterType;

        protected string _inputReplace = "PI_";
        protected string _outputReplace = "PO_";

        protected Exception _exceptionTemporary = null;

        protected string[] _reservedOutputName = new string[] { "PO_DATA", "PO_STATUS", "PO_STATUS_MSG", "PO_TOTAL" }; // ParameterName not to remove "PO_" when return


        protected string _connectionString = "";
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }

        protected ProviderFactory _provider = ProviderFactory.MSSQL;
        public ProviderFactory Provider
        {
            get
            {
                return _provider;
            }
            set
            {
                _provider = value;
            }
        }


        protected string _assemblyVersion = "";
        public string AssemblyVersion
        {
            get
            {
                return _assemblyVersion;
            }
            set
            {
                _assemblyVersion = value;
            }
        }


        protected System.Globalization.CultureInfo _cultureInfo = new System.Globalization.CultureInfo("th-TH");
        /// <summary>
        /// default is System.Globalization.CultureInfo("th-TH")
        /// </summary>
        public System.Globalization.CultureInfo CultureInfo
        {
            get { return _cultureInfo; }
            set { _cultureInfo = value; }
        }


        protected string _dateTimeFormat = "dd/MM/yyyy HH:mm:ss";
        /// <summary>
        /// default is "dd/MM/yyyy HH:mm:ss"
        /// </summary>
        public string DateTimeFormat
        {
            get { return _dateTimeFormat; }
            set { _dateTimeFormat = value; }
        }

        protected double _timeZoneOffset = -420;
        public double TimeZoneOffset
        {
            get { return _timeZoneOffset; }
            set { _timeZoneOffset = value; }
        }



        public DatabaseConnectorClass()
            : this(Core.GetConnectionString())
        {
        }
        public DatabaseConnectorClass(ProviderFactory provider)
            : this(null, provider)
        {
        }
        public DatabaseConnectorClass(System.Configuration.ConnectionStringSettings connectionStringSettings)
            : this(connectionStringSettings, null)
        {
        }
        public DatabaseConnectorClass(System.Configuration.ConnectionStringSettings connectionStringSettings, string version)
            : this((connectionStringSettings != null ? connectionStringSettings.ConnectionString : null),
            (connectionStringSettings.ProviderName.ToLower().Contains("sqlclient") ? ProviderFactory.MSSQL :
            connectionStringSettings.ProviderName.ToLower().Contains("oracle") ? ProviderFactory.Oracle :
            connectionStringSettings.ProviderName.ToLower().Contains("mysql") ? ProviderFactory.MySQL :
            connectionStringSettings.ProviderName.ToLower().Contains("npgsql") ? ProviderFactory.PostgreSQL :
            ProviderFactory.MSSQL))
        {
        }

        public DatabaseConnectorClass(string connectionString, ProviderFactory provider)
        {
            try
            {
                _connectionString = connectionString;
                _provider = provider;
                switch (_provider)
                {
                    case ProviderFactory.MSSQL:
                        _paramNamePrefix = "@";
                        _connectionType = typeof(System.Data.SqlClient.SqlConnection);
                        _commandType = typeof(System.Data.SqlClient.SqlCommand);
                        _commandBuilderType = typeof(System.Data.SqlClient.SqlCommandBuilder);
                        _parameterType = typeof(System.Data.SqlClient.SqlParameter);
                        break;
                    case ProviderFactory.Oracle:
                        _paramNamePrefix = string.Empty;
                        _assemblyName = _OracleAssembly;
                        _connectionTypeName = string.Format("{0}.Client.OracleConnection", _assemblyName);
                        _commandTypeName = string.Format("{0}.Client.OracleCommand", _assemblyName);
                        _commandBuilderTypeName = string.Format("{0}.Client.OracleCommandBuilder", _assemblyName);
                        _parameterTypeName = string.Format("{0}.Client.OracleParameter", _assemblyName);
                        break;
                    case ProviderFactory.MySQL:
                        _paramNamePrefix = string.Empty;
                        _assemblyName = _MySqlAssembly;
                        _connectionTypeName = "MySql.Data.MySqlClient.MySqlConnection";
                        _commandTypeName = "MySql.Data.MySqlClient.MySqlCommand";
                        _commandBuilderTypeName = "MySql.Data.MySqlClient.MySqlCommandBuilder";
                        _parameterTypeName = "MySql.Data.MySqlClient.MySqlParameter";
                        break;
                    case ProviderFactory.PostgreSQL:
                        _paramNamePrefix = ":";
                        _assemblyName = _PostgreSqlAssembly;
                        _connectionTypeName = "Npgsql.NpgsqlConnection";
                        _commandTypeName = "Npgsql.NpgsqlCommand";
                        _commandBuilderTypeName = "Npgsql.NpgsqlCommandBuilder";
                        _parameterTypeName = "Npgsql.NpgsqlParameter";
                        break;
                }
                if (_connectionType == null)
                {
                    if (!_dictAssemblyCache.ContainsKey(_assemblyName))
                    {
                        _dictAssemblyCache.TryAdd(_assemblyName, ConnectorUtil.AssemblyFromName(_assemblyName));
                    }
                    _connectionType = _dictAssemblyCache[_assemblyName].GetType(_connectionTypeName);
                    _commandType = _dictAssemblyCache[_assemblyName].GetType(_commandTypeName);
                    _commandBuilderType = _dictAssemblyCache[_assemblyName].GetType(_commandBuilderTypeName);
                    _parameterType = _dictAssemblyCache[_assemblyName].GetType(_parameterTypeName);
                }

                if (string.IsNullOrEmpty(_connectionString) && System.Configuration.ConfigurationManager.ConnectionStrings["dbConnection"] != null)
                    _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;

                if (!string.IsNullOrEmpty(_connectionString))
                {
                    _dbCoreConnection = Activator.CreateInstance(_connectionType, new object[] { _connectionString });
                }
                else
                {
                    _dbCoreConnection = Activator.CreateInstance(_connectionType);
                }
                _dbConnection = _dbCoreConnection as IDbConnection;
            }
            catch (Exception ex)
            {
                _exceptionTemporary = ex;
            }
        }

        protected IDbCommand CreateCommand(string storeName, QueryParameter queryParam, ref StringBuilder logString)
        {
            IDbDataParameter dbParam = null;
            //List<IDbDataParameter> paramList = null;
            IDbCommand dbCommand = null;
            string[] logHiddenParams = null;
            try
            {
                //paramList = new List<IDbDataParameter>();
                if (_dbConnection.State != ConnectionState.Open)
                {
                    _dbConnection.Open();
                }
                dbCommand = _dbConnection.CreateCommand();
                dbCommand.CommandType = CommandType.StoredProcedure;
                dbCommand.CommandText = storeName;
                if (_provider.Equals(ProviderFactory.Oracle))
                {
                    var oracleCommandBindByName = _commandType.GetProperty("BindByName");
                    oracleCommandBindByName.SetValue(dbCommand, true, null);
                }

                logHiddenParams = AppHttpHandler.AppHttpHandlerConfig.Logs.HideFields.ToArray();

                _commandBuilderType.GetMethod("DeriveParameters").Invoke(null, new object[] { dbCommand });
                for (int i = 0; i < dbCommand.Parameters.Count; i++)
                {
                    string paramName = string.Empty;
                    object paramValue = null;
                    dbParam = dbCommand.Parameters[i] as IDbDataParameter;
                    switch (dbParam.Direction)
                    {
                        case ParameterDirection.Input:
                            if (queryParam != null && queryParam.Parameter != null)
                            {
                                if (dbParam.ParameterName.IndexOf(_inputReplace) != -1)
                                {
                                    paramName = dbParam.ParameterName.Substring(dbParam.ParameterName.IndexOf(_inputReplace) + _inputReplace.Length);
                                }
                                else
                                {
                                    paramName = dbParam.ParameterName;
                                }

                                if (queryParam.Parameter.ContainsKey(paramName.ToUpper()))
                                {
                                    paramValue = queryParam.Parameter[paramName];
                                }

                                if (paramValue != null && ConnectorUtil.DBTypeMap(dbParam.DbType).Equals(typeof(DateTime)) || dbParam.Value is DateTime)
                                {
                                    if (paramValue is string)
                                    {
                                        long milliseconds;
                                        bool parseResult = long.TryParse((string)paramValue, out milliseconds);
                                        if (parseResult)
                                        {
                                            dbParam.Value = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(milliseconds).ToLocalTime();
                                        }
                                        else
                                        {
                                            dbParam.Value = ConnectorUtil.StringToDateTime(paramValue.ToString());
                                        }
                                    }
                                    else if (paramValue is int)
                                    {
                                        dbParam.Value = ConnectorUtil.UnixTimeStampToDateTime(int.Parse(paramValue.ToString())).AddMinutes(TimeZoneOffset);
                                    }
                                    else if (paramValue is long)
                                    {
                                        dbParam.Value = ConnectorUtil.UnixTimeStampToDateTime(long.Parse(paramValue.ToString())).AddMinutes(TimeZoneOffset);
                                    }

                                    if (_provider == ProviderFactory.MSSQL)
                                    {
                                        logString.AppendLine(string.Format("{0}: PARSE('{1}' as DATETIME USING 'th-TH')", dbParam.ParameterName, ConnectorUtil.DateTimeToString(dbParam.Value as DateTime?)));
                                    }
                                    else if (_provider == ProviderFactory.Oracle)
                                    {
                                        logString.AppendLine(string.Format("{0}: TO_DATE('{1}','DD/MM/YYYY HH24:MI:SS','NLS_DATE_LANGUAGE=THAI')", dbParam.ParameterName, ConnectorUtil.DateTimeToString(dbParam.Value as DateTime?)));
                                    }

                                    break;
                                }
                                else if (paramValue != null)
                                {
                                    if (dbParam.DbType != DbType.String && !string.IsNullOrEmpty(paramValue.ToString()))
                                    {
                                        dbParam.Value = paramValue;
                                    }
                                    else if (dbParam.DbType == DbType.String)
                                    {
                                        dbParam.Value = paramValue;
                                    }
                                    else
                                    {
                                    }
                                }
                                else
                                {
                                    dbParam.Value = System.DBNull.Value;
                                }
                            }
                            object logValue = dbParam.Value;
                            if (logHiddenParams != null && logHiddenParams.Contains(dbParam.ParameterName.Replace(_inputReplace, "").ToUpper()))
                            {
                                logValue = "-- Hidden Field --";
                            }
                            logString.AppendLine(string.Format("{0}: {1}", dbParam.ParameterName, logValue));
                            break;
                        case ParameterDirection.InputOutput:
                        case ParameterDirection.Output:
                            dbParam.Direction = ParameterDirection.Output;
                            break;
                        //paramName = dbParam.ParameterName.Substring(dbParam.ParameterName.IndexOf(_outputReplace) + _outputReplace.Length);
                        //break;
                        default:
                            continue;
                    }
                    //paramList.Add(dbParam);
                }
                return dbCommand;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public QueryResult ExecuteStoredProcedure(Stream requestStream)
        {
            QueryParameter queryParam = null;
            QueryResult queryResult = null;
            string storeName = string.Empty;
            try
            {
                queryParam = new QueryParameter(requestStream);
                queryResult = ExecuteStoredProcedure(queryParam);
            }
            catch (Exception ex)
            {
                queryResult = new QueryResult(ex);
            }
            return queryResult;
        }
        public QueryResult ExecuteStoredProcedure(QueryParameter queryParam)
        {
            QueryResult queryResult = null;
            string storeName = string.Empty;
            try
            {
                if (queryParam["APP_DATA_PROCEDURE"] != null)
                {
                    storeName = queryParam["APP_DATA_PROCEDURE"].ToString();
                    queryParam.Remove("APP_DATA_PROCEDURE");
                    queryResult = ExecuteStoredProcedure(storeName, queryParam);
                }
                else
                {
                    throw new Exception("ไม่ได้ระบุชื่อ store มาที่ parameter ชื่อ 'APP_DATA_PROCEDURE'");
                }

            }
            catch (Exception ex)
            {
                queryResult = new QueryResult(ex);
            }
            return queryResult;
        }
        public QueryResult ExecuteStoredProcedure(string storeName)
        {
            return ExecuteStoredProcedure(storeName, new QueryParameter());
        }
        public QueryResult ExecuteStoredProcedure(string storeName, Stream requestStream)
        {
            QueryParameter queryParam = null;
            QueryResult queryResult = null;
            try
            {
                queryParam = new QueryParameter(requestStream);
                queryResult = ExecuteStoredProcedure(storeName, queryParam);
            }
            catch (Exception ex)
            {
                queryResult = new QueryResult(ex);
            }
            return queryResult;
        }
        public QueryResult ExecuteStoredProcedure(string storeName, QueryParameter queryParam)
        {
            StringBuilder logString = null;
            QueryResult qResult = null;
            IDbCommand dbCommand = null;
            IDataReader dbReader = null;
            IDbDataParameter dbParam = null;
            //IDbTransaction dbTrans = null;
            DataTable dt = null;
            string paramName = string.Empty;
            int cursorNum = 1;
            try
            {
                logString = new StringBuilder();
                logString.AppendLine(string.Format("\tAPP_DATA_PROCEDURE: {0}", storeName));
                if (_dbConnection == null)
                {
                    if (_exceptionTemporary != null)
                    {
                        throw new Exception(string.Format("ไม่สามารถโหลด assembly {0} ได้เนื่องจาก\r\n{1}", _assemblyName, _exceptionTemporary.GetBaseException().Message));
                    }
                    else
                    {
                        throw new Exception(string.Format("ไม่สามารถโหลด assembly {0} ได้", _assemblyName));
                    }
                }
                _dbConnection.ConnectionString = _connectionString;
                if (_provider.Equals(ProviderFactory.MySQL))
                {
                    throw new Exception("Provider ไม่รองรับการ Execute โดยใช้ StoredProcedure");
                }
                if (_dbConnection.State != ConnectionState.Open)
                {
                    _dbConnection.Open();
                }
                qResult = new QueryResult();
                dbCommand = CreateCommand(storeName, queryParam, ref logString);
                //dbTrans = _dbConnection.BeginTransaction();
                switch (_provider)
                {
                    case ProviderFactory.MSSQL:
                        dbReader = dbCommand.ExecuteReader();
                        while (!dbReader.IsClosed)
                        {
                            dt = new DataTable();
                            dt.Load(dbReader);
                            if (cursorNum == 1)
                            {
                                qResult.DataTable = dt;
                            }
                            else
                            {
                                qResult.AddOutputParam("data" + cursorNum, ConnectorUtil.DataTableToDictionary(dt, _dateTimeFormat, _cultureInfo));
                            }
                            cursorNum++;
                        }
                        break;
                    default:
                        dbCommand.ExecuteNonQuery();
                        break;
                }
                for (int i = 0; i < dbCommand.Parameters.Count; i++)
                {
                    dbParam = dbCommand.Parameters[i] as IDbDataParameter;
                    if (!dbParam.Direction.Equals(ParameterDirection.Output)) continue;
                    paramName = dbParam.ParameterName;
                    if (_paramNamePrefix.Length > 0)
                    {
                        paramName = paramName.Replace(_paramNamePrefix, string.Empty);
                    }
                    if (!_reservedOutputName.Contains(paramName.ToUpper()))
                    {
                        paramName = paramName.Replace(_outputReplace, string.Empty);
                        if (dbParam.Value == null || System.DBNull.Value.Equals(dbParam.Value))
                        {
                            qResult.AddOutputParam(paramName, dbParam.Value);
                        }
                        else if (dbParam.Value is DateTime)
                        {
                            qResult.AddOutputParam(paramName, ConnectorUtil.DateTimeToString((dbParam.Value as DateTime?)));
                        }
                        else if (dbParam.Value is IDataReader)
                        {
                            dt = new DataTable();
                            if (dbParam.Value != System.DBNull.Value)
                            {
                                dt.Load(dbParam.Value as IDataReader);

                            }
                            qResult.AddOutputParam(paramName.ToLower(), ConnectorUtil.DataTableToDictionary(dt, _dateTimeFormat, _cultureInfo));
                        }
                        else
                        {
                            if (paramName.IndexOf("NT_") == 0)
                            {
                                qResult.AddNTParam(paramName, dbParam.Value);
                            }
                            else
                            {
                                qResult.AddOutputParam(paramName, dbParam.Value);
                            }
                        }
                    }
                    else if (paramName.Equals("PO_DATA"))
                    {
                        dt = new DataTable();
                        if (dbParam.Value != System.DBNull.Value)
                        {
                            dt.Load(dbParam.Value as IDataReader);
                        }
                        qResult.DataTable = dt;
                    }
                    else if (paramName.Equals("PO_TOTAL"))
                    {
                        if (!string.IsNullOrEmpty(dbParam.Value.ToString()))
                        {
                            qResult.Total = int.Parse(dbParam.Value.ToString());
                        }
                    }
                    else if (paramName.Equals("PO_STATUS"))
                    {
                        if (dbParam.Value.ToString().Equals("1"))
                        {
                            qResult.Success = true;
                        }
                        else
                        {
                            qResult.Success = false;
                        }
                    }
                    else if (paramName.Equals("PO_STATUS_MSG"))
                    {
                        qResult.Message = dbParam.Value.ToString();
                    }
                }

                logString.AppendLine(string.Format("<LOGFUNCTION>"));
                if (queryParam.Parameter.ContainsKey("FN_ID") == true)
                {
                    logString.AppendLine(string.Format("{0}", queryParam.Parameter["FN_ID"]));
                }
                logString.AppendLine(string.Format("</LOGFUNCTION>"));


                if (qResult.Success != true)
                {
                    logString.AppendLine(string.Format("{0}", qResult.Message));

                    logString.AppendLine(string.Format("<LOGSESSION>"));
                    logString.Append(LogSession());
                    logString.AppendLine(string.Format("</LOGSESSION>"));

                    logString.AppendLine("!!! Error !!!");
                    ConnectorUtil.WriteLogError(logString.ToString());
                }
                else
                {
                    logString.AppendLine(string.Format("<LOGSESSION>"));
                    logString.Append(LogSession());
                    logString.AppendLine(string.Format("</LOGSESSION>"));

                    logString.AppendLine("!!! Completed !!!");
                    ConnectorUtil.WriteLogInfo(logString.ToString());
                    //Util.WriteLogDebug(qResult.ToJson());
                }
                //try
                //{
                //    dbTrans.Commit();
                //}
                //catch { }
            }
            catch (Exception ex)
            {
                //try
                //{
                //    dbTrans.Rollback();
                //}
                //catch { }
                qResult = new QueryResult(ex);
                logString.AppendLine(string.Format("{0}", qResult.Message));

                logString.AppendLine(string.Format("<LOGFUNCTION>"));
                if (queryParam.Parameter.ContainsKey("FN_ID") == true)
                {
                    logString.AppendLine(string.Format("{0}", queryParam.Parameter["FN_ID"]));
                }
                logString.AppendLine(string.Format("</LOGFUNCTION>"));

                logString.AppendLine(string.Format("<LOGSESSION>"));
                logString.Append(LogSession());
                logString.AppendLine(string.Format("</LOGSESSION>"));

                logString.AppendLine("!!! Error !!!");
                ConnectorUtil.WriteLogError(logString.ToString());
            }
            finally
            {
                //try
                //{
                //    dbTrans.Dispose();
                //    dbTrans = null;
                //}
                //catch { }
                if (_dbConnection != null && _dbConnection.State == ConnectionState.Open)
                {
                    _dbConnection.Close();
                    _dbConnection.Dispose();
                }
            }
            return qResult;
        }


        protected IDbCommand CreateCommand(string StrSQL)
        {
            string StrSQL2 = StrSQL;
            IDbCommand _dbCommand = null;
            _dbCommand = _dbConnection.CreateCommand();
            _dbCommand.CommandType = CommandType.Text;

            if (_provider.Equals(ProviderFactory.Oracle))
            {
                var oracleCommandBindByName = _commandType.GetProperty("BindByName");
                oracleCommandBindByName.SetValue(_dbCommand, true, null);
            }

            string paramPrefix = _provider.Equals(ProviderFactory.MSSQL) ? "@PI_" : ":PI_";
            string[] paramName = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
            for (int i = StrSQL2.IndexOf("?"); i != -1; i = StrSQL2.IndexOf("?", i))
            {
                var regex = new System.Text.RegularExpressions.Regex(System.Text.RegularExpressions.Regex.Escape("?"));
                StrSQL = regex.Replace(StrSQL, paramPrefix + paramName[_dbCommand.Parameters.Count], 1);
                object _dbParam = Activator.CreateInstance(_parameterType, new object[] { "PI_" + paramName[_dbCommand.Parameters.Count], "" });
                _dbCommand.Parameters.Add(_dbParam);
                i++;
            }
            _dbCommand.CommandText = StrSQL;
            return _dbCommand;
        }
        public QueryResult ExecuteStatement(string statement)
        {
            return ExecuteStatement(statement, null);
        }
        public QueryResult ExecuteStatement(string statement, QueryParameter queryParam)
        {
            StringBuilder logString = null;
            QueryResult qResult = null;
            IDbCommand dbCommand = null;
            IDataReader dbReader = null;
            IDbDataParameter dbParam = null;
            DataTable dt = null;
            string paramName = string.Empty;
            int cursorNum = 1;
            try
            {
                logString = new StringBuilder();
                if (_dbConnection == null)
                {
                    throw new Exception(string.Format("ไม่สามารถโหลด assembly {0} ได้เนื่องจาก\r\n{1}", _assemblyName, _exceptionTemporary.GetBaseException().Message));
                }
                _dbConnection.ConnectionString = _connectionString;
                qResult = new QueryResult();
                if (_dbConnection.State != ConnectionState.Open)
                {
                    _dbConnection.Open();
                }
                dbCommand = CreateCommand(statement);
                if (queryParam != null)
                {
                    for (int i = 0; i < dbCommand.Parameters.Count; i++)
                    {
                        if (queryParam.Parameter.Count >= i)
                        {
                            dbParam = dbCommand.Parameters[i] as IDbDataParameter;
                            dbParam.Value = queryParam.Parameter[queryParam.Parameter.Keys.ElementAt(i)];
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                dbReader = dbCommand.ExecuteReader();
                while (!dbReader.IsClosed)
                {
                    dt = new DataTable();
                    dt.Load(dbReader);
                    if (cursorNum == 1)
                    {
                        qResult.DataTable = dt;
                    }
                    else
                    {
                        qResult.AddOutputParam("data" + cursorNum, ConnectorUtil.DataTableToDictionary(dt, _dateTimeFormat, _cultureInfo));
                    }
                    cursorNum++;
                }
                for (int i = 0; i < dbCommand.Parameters.Count; i++)
                {
                    dbParam = dbCommand.Parameters[i] as IDbDataParameter;
                    if (!dbParam.Direction.Equals(ParameterDirection.Output)) continue;
                    paramName = dbParam.ParameterName;
                    if (_paramNamePrefix.Length > 0)
                    {
                        paramName = paramName.Replace(_paramNamePrefix, string.Empty);
                    }
                    if (!_reservedOutputName.Contains(paramName.ToUpper()))
                    {
                        paramName = paramName.Replace(_outputReplace, string.Empty);
                        if (dbParam.Value == null || System.DBNull.Value.Equals(dbParam.Value))
                        {
                            qResult.AddOutputParam(paramName, dbParam.Value);
                        }
                        else if (dbParam.Value is DateTime)
                        {
                            qResult.AddOutputParam(paramName, ConnectorUtil.DateTimeToString((dbParam.Value as DateTime?)));
                        }
                        else if (dbParam.Value is IDataReader)
                        {
                            dt = new DataTable();
                            dt.Load(dbParam.Value as IDataReader);
                            qResult.AddOutputParam(paramName.ToLower(), ConnectorUtil.DataTableToDictionary(dt, _dateTimeFormat, _cultureInfo));
                        }
                        else
                        {
                            qResult.AddOutputParam(paramName, dbParam.Value);
                        }
                    }
                    else if (paramName.Equals("PO_DATA"))
                    {
                        dt = new DataTable();
                        dt.Load(dbParam.Value as IDataReader);
                        qResult.DataTable = dt;
                    }
                    else if (paramName.Equals("PO_TOTAL"))
                    {
                        if (!string.IsNullOrEmpty(dbParam.Value.ToString()))
                        {
                            qResult.Total = int.Parse(dbParam.Value.ToString());
                        }
                    }
                    else if (paramName.Equals("PO_STATUS"))
                    {
                        if (dbParam.Value.ToString().Equals("1"))
                        {
                            qResult.Success = true;
                        }
                        else
                        {
                            qResult.Success = false;
                        }
                    }
                    else if (paramName.Equals("PO_STATUS_MSG"))
                    {
                        qResult.Message = dbParam.Value.ToString();
                    }
                }

                logString.AppendLine(string.Format("<LOGFUNCTION>"));
                if (queryParam.Parameter.ContainsKey("FN_ID") == true)
                {
                    logString.AppendLine(string.Format("{0}", queryParam.Parameter["FN_ID"]));
                }
                logString.AppendLine(string.Format("</LOGFUNCTION>"));

                if (qResult.Success != true)
                {
                    logString.AppendLine(string.Format("{0}", qResult.Message));

                    logString.AppendLine(string.Format("<LOGSESSION>"));
                    logString.Append(LogSession());
                    logString.AppendLine(string.Format("</LOGSESSION>"));

                    logString.AppendLine("!!! Error !!!");
                    ConnectorUtil.WriteLogError(logString.ToString());
                }
                else
                {
                    logString.AppendLine(string.Format("<LOGSESSION>"));
                    logString.Append(LogSession());
                    logString.AppendLine(string.Format("</LOGSESSION>"));

                    logString.AppendLine("!!! Completed !!!");
                    ConnectorUtil.WriteLogInfo(logString.ToString());
                    //Util.WriteLogDebug(qResult.ToJson());
                }
            }
            catch (Exception ex)
            {
                qResult = new QueryResult(ex);
                logString.AppendLine(string.Format("{0}", qResult.Message));
                logString.AppendLine(string.Format("<LOGFUNCTION>"));
                if (queryParam.Parameter.ContainsKey("FN_ID") == true)
                {
                    logString.AppendLine(string.Format("{0}", queryParam.Parameter["FN_ID"]));
                }
                logString.AppendLine(string.Format("</LOGFUNCTION>"));
                logString.AppendLine(string.Format("<LOGSESSION>"));
                logString.Append(LogSession());
                logString.AppendLine(string.Format("</LOGSESSION>"));
                logString.AppendLine("!!! Error !!!");
                ConnectorUtil.WriteLogError(logString.ToString(), ex);
            }
            finally
            {
                if (_dbConnection != null && _dbConnection.State == ConnectionState.Open)
                {
                    _dbConnection.Close();
                }
            }
            return qResult;
        }

        private StringBuilder LogSession()
        {
            StringBuilder logString = new StringBuilder();
            string[] sessionFieldsToLog = null;

            sessionFieldsToLog = AppHttpHandler.AppHttpHandlerConfig.Logs.SessionFields.ToArray();
            for (int i = 0; i <= sessionFieldsToLog.Length - 1; i++)
            {
                try //POK ==> ครอบ try-catch ไว้ก่อนเพราะเกิด error ตอนส่ง notification 
                {
                    object sessionObj = AppSession.GetSession(sessionFieldsToLog[i]);
                    string sessionValue = "";
                    if (sessionObj != null)
                        sessionValue = sessionObj.ToString();

                    logString.AppendLine(string.Format("{0} : {1}", sessionFieldsToLog[i], sessionValue));
                }
                catch
                {

                }
            }

            return logString;
        }
    }
}
