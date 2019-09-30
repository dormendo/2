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
	class ReadLinkOraOriginal1 : ReadLinkOraTestBase
	{
		private OracleCommand _cmd;
		private OracleParameter _param;
		private OracleParameter _param2;

		private long _sum;

		public ReadLinkOraOriginal1(string name, int iterationCount, string connString)
			: base(name, iterationCount, 1, connString)
		{
			this._cmd = new OracleCommand("SELECT ObjectId FROM E52190_3 WHERE UserCode = :k1 AND State = 0 AND :d1 BETWEEN DocumentBeginDate AND DocumentEndDate", this._conn);
			this._param = new OracleParameter(":k1", OracleDbType.NVarchar2, 100);
			this._param2 = new OracleParameter(":d1", OracleDbType.Date);
			this._cmd.Parameters.Add(this._param);
			this._cmd.Parameters.Add(this._param2);
		}

		protected override void RunIteration(Stopwatch sw)
		{
			string code = this.GetSet()[0];
			sw.Start();

			this._param.Value = code;
			this._param2.Value = DateTime.Now;
			string objectId;
			using (OracleDataReader dr = this._cmd.ExecuteReader())
			{
				if (dr.Read())
				{
					objectId = dr.GetString(0);
				}
				else
				{
					objectId = "00000000-0000-0000-0000-000000000000";
				}
			}

			sw.Stop();

			byte[] ba = new Guid(objectId).ToByteArray();
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
