using Maxima;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportSqlSpeedUp
{
	abstract class SaveOraTestBase : DbOraTestBase
	{

		protected SaveOraTestBase(string name, int iterationCount, string connString) : base(name, iterationCount, connString)
		{
		}

		public override void Run()
		{
			this.TruncateTables();
			base.Run();
		}

		protected void TruncateTables()
		{
			using (OracleCommand cmd = new OracleCommand("TRUNCATE TABLE B", this._conn))
			{
				cmd.ExecuteNonQuery();
			}
			using (OracleCommand cmd = new OracleCommand("TRUNCATE TABLE C", this._conn))
			{
				cmd.ExecuteNonQuery();
			}
		}
	}
}
