using Maxima;
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
	class Save2Ora : SaveOraTestBase
	{
		int _rowCount;
		int _i;

		public Save2Ora(string name, int iterationCount, int rowCount, string connString) : base(name, iterationCount, connString)
		{
			this._rowCount = rowCount;
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
			sql1.AppendLine("BEGIN");
			for (int i = 0; i < this._rowCount; i++)
			{
				sql1.Append("INSERT /*+ append */ INTO IT_3(ID, DIRECTIVE, GUID, OBJECTID, PATCHID, USERCODE, DOCUMENTBEGINDATE, DOCUMENTENDDATE, ACTUALBEGINDATE, ACTUALENDDATE, STATE, ").
					Append("REMOVELEFT, REMOVERIGHT, OBJECTDEL, RECORDTYPE, C1783376938461729511, C8592504141572910447, C2089172047110907212, C5269410614712304966, ").
					Append("C1095156487921478956, C4437532284984408449, C2931805783443429356, C1245021195155043293, C7241011763022076318) VALUES ");
				sql1.Append("(");
				sql1.Append(++this._i).Append(",");
				sql1.Append(3).Append(", ");
				sql1.Append("'").Append(Guid.NewGuid()).Append("'").Append(", ");
				sql1.Append("'").Append(Guid.NewGuid()).Append("'").Append(", ");
				sql1.Append(3).Append(", ");
				sql1.Append("'").Append(this._currentIteration).Append("'").Append(", ");
				sql1.Append("TO_DATE('").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("', 'YYYY-MM-dd HH24:MI:SS'), ");
				sql1.Append("TO_DATE('").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("', 'YYYY-MM-dd HH24:MI:SS'), ");
				sql1.Append("TO_DATE('").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("', 'YYYY-MM-dd HH24:MI:SS'), ");
				sql1.Append("TO_DATE('").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("', 'YYYY-MM-dd HH24:MI:SS'), ");
				sql1.Append("NULL").Append(", ");
				sql1.Append(0).Append(", ");
				sql1.Append(0).Append(", ");
				sql1.Append(0).Append(", ");
				sql1.Append(0).Append(", ");
				sql1.Append("'").Append(this._currentIteration).Append("'").Append(", ");
				sql1.Append("'").Append(Guid.NewGuid()).Append("',");
				sql1.Append(this._currentIteration).Append(",");
				sql1.Append("'").Append(this._currentIteration).Append("'").Append(", ");
				sql1.Append("'").Append(this._currentIteration).Append("'").Append(", ");
				sql1.Append("'").Append(this._currentIteration).Append("'").Append(", ");
				sql1.Append("'").Append(this._currentIteration / 100).Append("'").Append(", ");
				sql1.Append("'").Append(this._currentIteration / 100).Append("'").Append(", ");
				sql1.Append("'").Append(this._currentIteration).Append("'");
				sql1.AppendLine(");");
			}

			for (int i = 0; i < this._rowCount; i++)
			{
				sql1.Append("INSERT /*+ append */ INTO IT_4(ID, DEDUP_STR_1, DEDUP_STR_2, DEDUP_STR_3) VALUES ");
				sql1.Append("(");
				sql1.Append("'").Append(Guid.NewGuid()).Append("'").Append(", ");
				sql1.Append("'").Append(Guid.NewGuid()).Append("'").Append(", ");
				sql1.Append("'").Append(Guid.NewGuid()).Append("'").Append(", ");
				sql1.Append("'").Append(Guid.NewGuid()).Append("'");
				sql1.AppendLine(");");
			}

			sql1.AppendLine("COMMIT;");
			sql1.AppendLine("END;");

			using (OracleCommand cmd = new OracleCommand(sql1.ToString(), this._conn))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public override void Dispose()
		{
			base.Dispose();
		}
	}
}
