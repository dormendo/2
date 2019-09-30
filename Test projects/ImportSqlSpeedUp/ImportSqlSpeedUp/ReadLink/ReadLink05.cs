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
	class ReadLink05 : ReadLinkTestBase
	{
		private List<string> _idList;

		private SqlCommand _cmd;

		private long _sum;

		private Dictionary<string, Guid> _foundKeyDict = new Dictionary<string, Guid>();

		private StructuredParamValue _codes;

		public ReadLink05(string name, int iterationCount, int rowCount, string connString)
			: base(name, iterationCount, rowCount, connString)
		{
			this._cmd = new SqlCommand("SELECT DISTINCT e.UserCode, e.ObjectId FROM E11696 e INNER JOIN @t1 t ON t.UserCode = e.UserCode WHERE e.State = 0", this._conn);
			this._codes = new StructuredParamValue(new[] { new SqlMetaData("UserCode", SqlDbType.NVarChar, 10) }, rowCount);
			this._cmd.AddStructuredParam("@t1", "dbo.E11696_ImportType", this._codes);
		}

		protected override void RunIteration(Stopwatch sw)
		{
			List<string> codes = this.GetSet();
			sw.Start();

			_foundKeyDict.Clear();

			this._codes.Clear();
			foreach (string code in codes)
			{
				this._codes.NewRecord();
				this._codes.AddString(code);
			}

			//using (SqlTransaction tx = this._conn.BeginTransaction())
			//{
			//	this._cmd.Transaction = tx;
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

			//	tx.Commit();
			//}

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
