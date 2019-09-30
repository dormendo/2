using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Maxima
{
    public static class SqlServer
    {
        // таймаут увеличен (c 10сек) для прохождения тестов выполняющихся после RESTORE DATABASE
        // таймаут увеличен (c 10сек) BS-1161
        private const int DefaultCommandTimeout = 30;

        private static readonly string _mainConnectionString;
        private static readonly string _mainTaskConnectionString;
        private static readonly Dictionary<string, string> _connectionStrings = new Dictionary<string, string>();

        [ThreadStatic]
        private static int _scopeCount;
        
        static SqlServer()
        {
            foreach (ConnectionStringSettings css in ConfigurationManager.ConnectionStrings)
            {
                if (string.IsNullOrWhiteSpace(css.ProviderName) || string.CompareOrdinal(css.ProviderName, "System.Data.SqlClient") == 0)
                {
                    _connectionStrings.Add(css.Name, css.ConnectionString);
                }
            }

            ConnectionStringSettings mainCss = ConfigurationManager.ConnectionStrings[ConnectionStringKeys.Main];
            if (mainCss != null)
            {
                _mainConnectionString = mainCss.ConnectionString;
            }
            ConnectionStringSettings mainTaskCss = ConfigurationManager.ConnectionStrings[ConnectionStringKeys.MainTasks];
            if (mainTaskCss != null)
            {
                _mainTaskConnectionString = mainTaskCss.ConnectionString;
            }
        }

        public static bool IsMainConnectionAvailable
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_mainConnectionString);
            }
        }

        public static string MainConnectionString
        {
            get
            {
                return _mainConnectionString;
            }
        }

        public static SqlConnection GetConnection(string connectionKey)
        {
            string connectionString;

            if (connectionKey == ConnectionStringKeys.Main && IsMainConnectionAvailable)
            {
                connectionString = _mainConnectionString;
            }
            else if (!_connectionStrings.TryGetValue(connectionKey, out connectionString))
            {
                throw new ArgumentException(connectionKey);
            }

            return new SqlConnection(connectionString);
        }

        public static SqlConnection GetConnection()
        {
            return GetConnection(_scopeCount == 0 ? ConnectionStringKeys.Main : ConnectionStringKeys.MainTasks);
        }

        public static SqlCommand GetCommand(string queryText, SqlConnection connection, SqlTransaction transaction)
        {
            return new SqlCommand(queryText, connection, transaction)
            {
                CommandType = CommandType.Text,
                CommandTimeout = DefaultCommandTimeout
            };
        }

        public static SqlCommand GetCommand(string queryText, SqlConnection connection)
        {
            return new SqlCommand(queryText, connection)
            {
                CommandType = CommandType.Text,
                CommandTimeout = DefaultCommandTimeout
            };
        }

        public static SqlCommand GetCommand(string queryText)
        {
            return new SqlCommand(queryText)
            {
                CommandType = CommandType.Text,
                CommandTimeout = DefaultCommandTimeout
            };
        }

        public static SqlCommand GetSpCommand(string spName, SqlConnection connection, SqlTransaction transaction)
        {
            return new SqlCommand(spName, connection, transaction)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = DefaultCommandTimeout
            };
        }

        public static SqlCommand GetSpCommand(string spName, SqlConnection connection)
        {
            return new SqlCommand(spName, connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = DefaultCommandTimeout
            };
        }

        public static SqlCommand GetSpCommand(string spName)
        {
            return new SqlCommand(spName)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = DefaultCommandTimeout
            };
        }

        #region Названия строк соединения

        public static class ConnectionStringKeys
        {
            public const string Main = "Main";

            public const string MainTasks = "MainTasks";

            public const string Arh = "Arh";
        }

        #endregion

        private class SystemTaskScope : IDisposable
        {
            public SystemTaskScope()
            {
                SqlServer._scopeCount++;
            }

            public void Dispose()
            {
                SqlServer._scopeCount--;
            }
        }

        public static IDisposable AcquireSystemTaskScope()
        {
            return new SystemTaskScope();
        }

        public static string QuoteString(string str)
        {
            if (str == null)
            {
                return "[]";
            }

            return "[" + str.Replace("]", "]]") + "]";
        }
    }
}