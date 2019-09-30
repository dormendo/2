using Maxima;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportSqlSpeedUp
{
	class Save2 : SaveTestBase
	{
		SqlCommand _beginTran;
		SqlCommand _commitTran;
		int _rowCount;

		public Save2(string name, int iterationCount, int rowCount, string connString) : base(name, iterationCount, connString)
		{
			this._rowCount = rowCount;

			this._beginTran = SqlServer.GetCommand("BEGIN TRANSACTION", this._conn);
			this._commitTran = SqlServer.GetCommand("COMMIT TRANSACTION WITH (DELAYED_DURABILITY = ON)", this._conn);
		}

		protected override void RunIteration(Stopwatch sw)
		{
			sw.Start();

			for (int i = 0; i < this._rowCount; i++)
			{
				this.Save();
				this._singleOperations++;
			}

			sw.Stop();
		}

		protected void Save()
		{
			StringBuilder sql1 = new StringBuilder();
			sql1.AppendLine("SET NOCOUNT ON; BEGIN TRANSACTION");
			for (int i = 0; i < this._rowCount; i++)
			{
				sql1.Append("INSERT INTO IT_6819804243173122816(Directive, GUID, ObjectID, PatchID, UserCode, DocumentBeginDate, DocumentEndDate, ActualBeginDate, ActualEndDate, State, RemoveLeft, RemoveRight, ObjectDel, RecordType, C1783376938461729511, C8592504141572910447) VALUES ");
				sql1.Append("(");
				sql1.Append(3).Append(", ");
				sql1.Append("'").Append(Guid.NewGuid()).Append("'").Append(", ");
				sql1.Append("'").Append(Guid.NewGuid()).Append("'").Append(", ");
				sql1.Append(3).Append(", ");
				sql1.Append("'").Append(this._currentIteration).Append("'").Append(", ");
				sql1.Append("CONVERT(datetime, '").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("', 120), ");
				sql1.Append("CONVERT(datetime, '").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("', 120), ");
				sql1.Append("CONVERT(datetime, '").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("', 120), ");
				sql1.Append("CONVERT(datetime, '").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("', 120), ");
				sql1.Append("NULL").Append(", ");
				sql1.Append(0).Append(", ");
				sql1.Append(0).Append(", ");
				sql1.Append(0).Append(", ");
				sql1.Append(0).Append(", ");
				sql1.Append("'").Append(this._currentIteration).Append("'").Append(", ");
				sql1.Append("'").Append(Guid.NewGuid()).Append("'");
				sql1.AppendLine(");");
			}

			for (int i = 0; i < this._rowCount; i++)
			{
				sql1.Append("INSERT INTO IT_5278945729007601847(ID, DEDUP_STR_1, DEDUP_STR_2, DEDUP_STR_3) VALUES ");
				sql1.Append("(");
				sql1.Append("'").Append(Guid.NewGuid()).Append("'").Append(", ");
				sql1.Append("'").Append(Guid.NewGuid()).Append("'").Append(", ");
				sql1.Append("'").Append(Guid.NewGuid()).Append("'").Append(", ");
				sql1.Append("'").Append(Guid.NewGuid()).Append("'");
				sql1.AppendLine(");");
			}

			sql1.AppendLine("COMMIT TRANSACTION WITH (DELAYED_DURABILITY = ON)");

			using (SqlCommand cmd = SqlServer.GetCommand(sql1.ToString(), this._conn))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public override void Dispose()
		{
			if (this._beginTran != null)
			{
				this._beginTran.Dispose();
				this._beginTran = null;
			}
			if (this._commitTran != null)
			{
				this._commitTran.Dispose();
				this._commitTran = null;
			}
			base.Dispose();
		}
	}
}
