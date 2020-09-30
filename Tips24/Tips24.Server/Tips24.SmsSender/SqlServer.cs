using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Tips24.SmsSender
{
	public class SqlServer : DataAccess.SqlServer
	{
		private static string _connectionString;

		static SqlServer()
		{
			_connectionString = Program.Config.GetSection("ConnectionStrings:regular").Value;
		}

		public SqlConnection GetConnection()
		{
			return this.GetConnection(_connectionString);
		}
	}
}
