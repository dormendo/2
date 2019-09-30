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
	class ReadLinkOra031 : ReadLinkOraTestBase
	{
		private OracleCommand _cmd;

		private long _sum;

		private Dictionary<string, string> _foundKeyDict = new Dictionary<string, string>();

		public ReadLinkOra031(string name, int iterationCount, int rowCount, string connString)
			: base(name, iterationCount, rowCount, connString)
		{
		}

		protected override void RunIteration(Stopwatch sw)
		{
			List<string> codes = this.GetSet();
			sw.Start();

			_foundKeyDict.Clear();

			StringBuilder sb = new StringBuilder("SELECT e.UserCode, e.ObjectId, a.RowIndex FROM E52190_3 e INNER JOIN (");
			for (int i = 0; i < codes.Count; i++)
			{
				if (i > 0)
				{
					sb.Append(" UNION ALL ");
				}
				sb.Append("SELECT N'").Append(codes[i].Replace("'", "''")).Append("' UserCode, TO_DATE('").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("', 'YYYY-MM-dd HH24:MI:SS') DocumentBeginDate, ").Append(i).Append(" RowIndex FROM dual");
			}

			sb.Append(") a ON e.UserCode = a.UserCode AND a.DocumentBeginDate BETWEEN e.DocumentBeginDate AND e.DocumentEndDate WHERE e.State = 0");

			using (this._cmd = new OracleCommand(sb.ToString(), this._conn))
			{
				using (OracleDataReader dr = this._cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						string foundCode = dr.GetString(0);
						string objectId = dr.GetString(1);
						int rowIndex = dr.GetInt32(2);
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
