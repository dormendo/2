using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportSqlSpeedUp
{
	abstract class DbTestBase : TestBase
	{
		protected SqlConnection _conn;

		protected DbTestBase(string name, int iterationCount, string connString)
			: base(name, iterationCount)
		{
			this._conn = new SqlConnection(connString);
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
