using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Maxima.DataAccess
{
    public class AdoWorker
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        protected SqlConnection Connection;
        protected SqlTransaction Transaction;
        private TimeSpan _commandTimeout;

        public AdoWorker(SqlConnection conn, SqlTransaction transaction)
        {
            if (conn.State != ConnectionState.Open)
                throw new ArgumentException("Connection must be opened");
            Connection = conn;
            Transaction = transaction;
            CommandTimeout = DefaultTimeout;
        }

        public TimeSpan CommandTimeout
        {
            get { return _commandTimeout; }
            set
            {
                if (value <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("value", value, "Timeout must be positive");
                _commandTimeout = value;
            }
        }

        public int ExecuteCommand(string commandText, params object[] @params)
        {
            AssertValidConnectionState(Connection);
            using (var cmd = GetCommand(commandText))
            {
                var outParams = SetParameters(cmd, @params, false);
                var rowsNum = cmd.ExecuteNonQuery();
                outParams.ForEach(p => p.ReadOutputValue());
                return rowsNum;
            }
        }

        /// <param name="spName">Stored procedure name, optionally qualified with schema name.</param>
        /// <param name="params">Pass an instance of <see cref="Maxima.DataAccess.OutParameter{T}"/> to get a value of a stored procedure output parameter.</param>
        public void ExecuteStoredProcedureCommand(string spName, params Tuple<string, object>[] @params)
        {
            if (string.IsNullOrEmpty(spName))
                throw new ArgumentException("Stored procedure name can't be empty", "spName");
            AssertValidConnectionState(Connection);
            using (var cmd = GetCommand(spName))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                var outParams = SetParameters(cmd, @params, true);
                cmd.ExecuteNonQuery();
                outParams.ForEach(p => p.ReadOutputValue());
            }
        }

        protected static void AssertValidConnectionState(SqlConnection conn)
        {
            if (conn.State != ConnectionState.Open)
                throw new InvalidOperationException();
        }

        protected static List<OutParameterBase> SetParameters(SqlCommand cmd, object[] args, bool namedParams)
        {
            var outParams = new List<OutParameterBase>();
            if (args == null)
                return outParams;
            
            for (var i = 0; i < args.Length; i++)
            {
                var paramName = namedParams ? ((Tuple<string, object>)args[i]).Item1 : "@" + i;
                var arg = namedParams ? ((Tuple<string, object>)args[i]).Item2 : args[i];
                var param = new SqlParameter { ParameterName = paramName };
                if (arg is ComplexType)
                {
                    var dt = ((ComplexType)arg).ToDataTable();
                    param.Value = dt;
                    param.TypeName = dt.TableName;
                }
                else if (arg is OutParameterBase)
                {
                    var outParam = (OutParameterBase)arg;
                    outParam.BindToParameter(param);
                    outParams.Add(outParam);
                }
                else
                {
                    param.Value = arg ?? DBNull.Value;
                }

                cmd.Parameters.Add(param);
            }

            return outParams;
        }

        protected SqlCommand GetCommand(string commandText)
        {
            var cmd = SqlServer.GetCommand(commandText, Connection);
            cmd.Transaction = Transaction;
            cmd.CommandTimeout = (int) CommandTimeout.TotalSeconds;
            return cmd;
        }
    }
}