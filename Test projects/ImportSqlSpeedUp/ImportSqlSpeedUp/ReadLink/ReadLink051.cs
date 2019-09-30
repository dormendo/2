using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Maxima;
using Microsoft.SqlServer.Server;

namespace ImportSqlSpeedUp
{
	class ReadLink051 : ReadLinkTestBase
	{
		private SqlCommand _cmd;

		private long _sum;

		private Dictionary<string, Guid> _foundKeyDict = new Dictionary<string, Guid>();

		private StructuredParamValue _codes;

		public ReadLink051(string name, int iterationCount, int rowCount, string connString)
			: base(name, iterationCount, rowCount, connString)
		{
			this._cmd = new SqlCommand("SELECT e.UserCode, e.ObjectId, t.RowIndex FROM E11696 e INNER JOIN @t1 t ON t.UserCode = e.UserCode AND t.DocumentBeginDate BETWEEN e.DocumentBeginDate AND e.DocumentEndDate WHERE e.State = 0", this._conn);
			this._codes = new StructuredParamValue(
				new[]
				{
					new SqlMetaData("UserCode", SqlDbType.NVarChar, 10),
					new SqlMetaData("DocumentBeginDate", SqlDbType.DateTime),
					new SqlMetaData("RowIndex", SqlDbType.Int)
				}, rowCount);
			this._cmd.AddStructuredParam("@t1", "dbo.E11696_ImportType1", this._codes);
		}

		protected override void RunIteration(Stopwatch sw)
		{
			List<string> codes = this.GetSet();
			sw.Start();

			_foundKeyDict.Clear();

			this._codes.Clear();
			for (int i = 0; i < codes.Count; i++)
			{
				string code = codes[i];
				this._codes.NewRecord();
				this._codes.AddString(code);
				this._codes.AddDateTime(DateTime.Now);
				this._codes.AddInt32(i);
			}

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
