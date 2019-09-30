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
	class ReadLinkOra03 : ReadLinkOraTestBase
	{
		private OracleCommand _cmd;

		private long _sum;

		private Dictionary<string, string> _foundKeyDict = new Dictionary<string, string>();

		public ReadLinkOra03(string name, int iterationCount, int rowCount, string connString)
			: base(name, iterationCount, rowCount, connString)
		{
		}

		protected override void RunIteration(Stopwatch sw)
		{
			List<string> codes = this.GetSet();
			sw.Start();

			_foundKeyDict.Clear();
			using (this._cmd = new OracleCommand("SELECT UserCode, ObjectId FROM (SELECT e.UserCode, e.ObjectId, ROW_NUMBER()OVER(PARTITION BY e.UserCode ORDER BY NULL) rn FROM E52190_3 e INNER JOIN ( SELECT N'" + string.Join("' UserCode FROM dual UNION ALL SELECT N'", codes) + "' UserCode FROM dual) a ON e.UserCode = a.UserCode WHERE e.State = 0) a WHERE rn = 1", this._conn))
			{
				using (OracleDataReader dr = this._cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						string foundCode = dr.GetString(0);
						string objectId = dr.GetString(1);
						_foundKeyDict.Add(foundCode, objectId);
						this._singleOperations++;
					}
				}
			}

			for (int i = 0; i < codes.Count; i++)
			{
				if (_foundKeyDict.ContainsKey(codes[i]))
				{
					this._sum++;
				}
			}

			sw.Stop();
		}

		public long Sum
		{
			get
			{
				return this._sum;
			}
		}

		public override void Dispose()
		{
			if (this._cmd != null)
			{
				this._cmd.Dispose();
				this._cmd = null;
			}

			base.Dispose();
		}
	}
}
