using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportSqlSpeedUp
{
	class ReadLink03 : ReadLinkTestBase
	{
		private List<string> _idList;

		private SqlCommand _cmd;

		private long _sum;

		private Dictionary<string, Guid> _foundKeyDict = new Dictionary<string, Guid>();

		public ReadLink03(string name, int iterationCount, int rowCount, string connString)
			: base(name, iterationCount, rowCount, connString)
		{
		}

		protected override void RunIteration(Stopwatch sw)
		{
			List<string> codes = this.GetSet();
			sw.Start();

			_foundKeyDict.Clear();
			using (this._cmd = new SqlCommand("SELECT UserCode, ObjectId FROM (SELECT e.UserCode, e.ObjectId, ROW_NUMBER()OVER(PARTITION BY e.UserCode ORDER BY (SELECT NULL)) rn FROM E11696 e INNER JOIN ( VALUES (N'" + string.Join("'), (N'", codes) + "')) a (UserCode) ON e.UserCode = a.UserCode WHERE e.State = 0) a WHERE rn = 1", this._conn))
			{
				using (SqlDataReader dr = this._cmd.ExecuteReader())
				{
					while (dr.Read())
					{
						string foundCode = dr.GetString(0);
						Guid objectId = dr.GetGuid(1);
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
