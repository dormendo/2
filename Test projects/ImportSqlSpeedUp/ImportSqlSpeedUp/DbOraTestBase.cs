using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportSqlSpeedUp
{
	abstract class DbOraTestBase : TestBase
	{
		protected OracleConnection _conn;

		protected DbOraTestBase(string name, int iterationCount, string connString)
			: base(name, iterationCount)
		{
			this._conn = new OracleConnection(connString);
			this._conn.Open();
		}

		public override void Dispose()
		{
			if (this._conn != null)
			{
				this._conn.Close();
				this._conn = null;
			}
		}
	}
}
