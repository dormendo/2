using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportSqlSpeedUp
{
	class ReadLink031 : ReadLinkTestBase
	{
		private List<string> _idList;

		private SqlCommand _cmd;

		private long _sum;

		private Dictionary<string, Guid> _foundKeyDict = new Dictionary<string, Guid>();

		public ReadLink031(string name, int iterationCount, int rowCount, string connString)
			: base(name, iterationCount, rowCount, connString)
		{
		}

		protected override void RunIteration(Stopwatch sw)
		{
			List<string> codes = this.GetSet();
			sw.Start();

			_foundKeyDict.Clear();

			StringBuilder sb = new StringBuilder("SELECT e.UserCode, e.ObjectId, a.RowIndex FROM E11696 e INNER JOIN ( VALUES ");
			for (int i = 0; i < codes.Count; i++)
			{
				if (i > 0)
				{
					sb.Append(", ");
				}
				sb.Append("(N'").Append(codes[i].Replace("'", "''")).Append("', CONVERT(datetime, '").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).
					Append("', 120), ").Append(i).Append(")");
			}

			sb.Append(") a (UserCode, DocumentBeginDate, RowIndex) ON e.UserCode = a.UserCode AND a.DocumentBeginDate BETWEEN e.DocumentBeginDate AND e.DocumentEndDate WHERE e.State = 0");

			using (this._cmd = new SqlCommand(sb.ToString(), this._conn))
			{
				using (SqlDataReader dr = this._cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						string foundCode = dr.GetString(0);
						Guid objectId = dr.GetGuid(1);
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
