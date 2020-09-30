using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Tips24.DataAccess
{
	public abstract class SqlServer
	{
		protected const int DefaultCommandTimeout = 30;

		public SqlConnection GetConnection(string connectionString)
		{
			return new SqlConnection(connectionString);
		}

		public SqlCommand GetCommand(string queryText, SqlConnection connection, SqlTransaction transaction)
		{
			return new SqlCommand(queryText, connection, transaction)
			{
				CommandType = CommandType.Text,
				CommandTimeout = DefaultCommandTimeout
			};
		}

		public SqlCommand GetCommand(string queryText, SqlConnection connection)
		{
			return new SqlCommand(queryText, connection)
			{
				CommandType = CommandType.Text,
				CommandTimeout = DefaultCommandTimeout
			};
		}

		public SqlCommand GetCommand(string queryText)
		{
			return new SqlCommand(queryText)
			{
				CommandType = CommandType.Text,
				CommandTimeout = DefaultCommandTimeout
			};
		}

		public SqlCommand GetSpCommand(string spName, SqlConnection connection, SqlTransaction transaction)
		{
			return new SqlCommand(spName, connection, transaction)
			{
				CommandType = CommandType.StoredProcedure,
				CommandTimeout = DefaultCommandTimeout
			};
		}

		public SqlCommand GetSpCommand(string spName, SqlConnection connection)
		{
			return new SqlCommand(spName, connection)
			{
				CommandType = CommandType.StoredProcedure,
				CommandTimeout = DefaultCommandTimeout
			};
		}

		public SqlCommand GetSpCommand(string spName)
		{
			return new SqlCommand(spName)
			{
				CommandType = CommandType.StoredProcedure,
				CommandTimeout = DefaultCommandTimeout
			};
		}

		public string QuoteString(string str)
		{
			if (str == null)
			{
				return "[]";
			}

			return "[" + str.Replace("]", "]]") + "]";
		}
	}
}