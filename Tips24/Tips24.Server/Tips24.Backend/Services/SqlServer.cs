using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Tips24
{
	public class SqlServer : DataAccess.SqlServer
	{
		private readonly string _regularConnectionString;

		public SqlServer(IConfiguration config)
		{
			this._regularConnectionString = config.GetValue<string>("Database:regular");
		}

		public SqlConnection GetConnection()
		{
			return GetConnection(_regularConnectionString);
		}
	}
}
