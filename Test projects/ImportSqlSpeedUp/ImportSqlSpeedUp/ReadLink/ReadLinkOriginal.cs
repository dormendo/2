using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportSqlSpeedUp
{
	class ReadLinkOriginal : ReadLinkTestBase
	{
		private SqlCommand _cmd;
		private SqlParameter _param;

		private long _sum;

		public ReadLinkOriginal(string name, int iterationCount, string connString)
			: base(name, iterationCount, 1, connString)
		{
			this._cmd = new SqlCommand("SELECT TOP 1 ObjectId FROM E11696 WHERE UserCode = @k1 AND State = 0", this._conn);
			this._param = new SqlParameter("@k1", System.Data.SqlDbType.NVarChar, 10);
			this._cmd.Parameters.Add(this._param);
		}

		protected override void RunIteration(Stopwatch sw)
		{
			string code = this.GetSet()[0];
			sw.Start();
			this._param.Value = code;

			Guid objectId;
			using (SqlDataReader dr = this._cmd.ExecuteReader())
			{
				if (dr.Read())
				{
					objectId = dr.GetGuid(0);
				}
				else
				{
					objectId = Guid.Empty;
				}
			}

			sw.Stop();

			byte[] ba = objectId.ToByteArray();
			_sum += (int)ba[0] + (int)ba[9];
			this._singleOperations++;
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
