using Maxima;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportSqlSpeedUp
{
	class Save3 : SaveTestBase
	{
		SqlCommand _beginTran;
		SqlCommand _commitTran;
		int _rowCount;

		private StructuredParamValue _positions;
		private StructuredParamValue _dedup;

		SqlCommand _savePosition;
		SqlCommand _saveDedup;

		public Save3(string name, int iterationCount, int rowCount, string connString) : base(name, iterationCount, connString)
		{
			this._rowCount = rowCount;

			this._positions = new StructuredParamValue(
				new[]
				{
					new SqlMetaData("Directive", SqlDbType.Int),
					new SqlMetaData("GUID", SqlDbType.UniqueIdentifier),
					new SqlMetaData("ObjectID", SqlDbType.UniqueIdentifier),
					new SqlMetaData("PatchID", SqlDbType.Int),
					new SqlMetaData("UserCode", SqlDbType.NVarChar, 10),
					new SqlMetaData("DocumentBeginDate", SqlDbType.DateTime),
					new SqlMetaData("DocumentEndDate", SqlDbType.DateTime),
					new SqlMetaData("ActualBeginDate", SqlDbType.DateTime),
					new SqlMetaData("ActualEndDate", SqlDbType.DateTime),
					new SqlMetaData("State", SqlDbType.Int),
					new SqlMetaData("RemoveLeft", SqlDbType.Bit),
					new SqlMetaData("RemoveRight", SqlDbType.Bit),
					new SqlMetaData("ObjectDel", SqlDbType.Int),
					new SqlMetaData("RecordType", SqlDbType.Int),
					new SqlMetaData("C1783376938461729511", SqlDbType.NVarChar, 10),
					new SqlMetaData("C8592504141572910447", SqlDbType.NVarChar, 50)
				}, rowCount);

			this._dedup = new StructuredParamValue(
				new[]
				{
					new SqlMetaData("ID", SqlDbType.UniqueIdentifier),
					new SqlMetaData("DEDUP_STR_1", SqlDbType.NVarChar, -1),
					new SqlMetaData("DEDUP_STR_2", SqlDbType.NVarChar, -1),
					new SqlMetaData("DEDUP_STR_3", SqlDbType.NVarChar, -1)
				}, rowCount);

			this._beginTran = SqlServer.GetCommand("BEGIN TRANSACTION", this._conn);
			this._commitTran = SqlServer.GetCommand("COMMIT TRANSACTION WITH (DELAYED_DURABILITY = ON)", this._conn);

			StringBuilder sql1 = new StringBuilder();
			sql1.AppendLine("INSERT INTO IT_6819804243173122816(Directive, GUID, ObjectID, PatchID, UserCode, DocumentBeginDate, DocumentEndDate, ActualBeginDate, ActualEndDate, State, RemoveLeft, RemoveRight, ObjectDel, RecordType, C1783376938461729511, C8592504141572910447)");
			sql1.Append("SELECT Directive, GUID, ObjectID, PatchID, UserCode, DocumentBeginDate, DocumentEndDate, ActualBeginDate, ActualEndDate, State, RemoveLeft, RemoveRight, ObjectDel, RecordType, C1783376938461729511, C8592504141572910447 FROM @t1");

			this._savePosition = SqlServer.GetCommand(sql1.ToString(), this._conn);
			this._savePosition.AddStructuredParam("@t1", "dbo.IT_6819804243173122816_Positions", this._positions);

			StringBuilder sql2 = new StringBuilder();
			sql2.AppendLine("INSERT INTO IT_5278945729007601847(ID, DEDUP_STR_1, DEDUP_STR_2, DEDUP_STR_3)");
			sql2.Append("SELECT ID, DEDUP_STR_1, DEDUP_STR_2, DEDUP_STR_3 FROM @t1");

			this._saveDedup = SqlServer.GetCommand(sql2.ToString(), this._conn);
			this._saveDedup.AddStructuredParam("@t1", "dbo.IT_5278945729007601847_Dedup", this._dedup);
		}

		protected override void RunIteration(Stopwatch sw)
		{
			sw.Start();

			this._positions.Clear();
			this._dedup.Clear();
			for (int i = 0; i < this._rowCount; i++)
			{
				this._positions.NewRecord();
				this._positions.AddInt32(3);
				this._positions.AddGuid(Guid.NewGuid());
				this._positions.AddGuid(Guid.NewGuid());
				this._positions.AddInt32(3);
				this._positions.AddString(this._currentIteration.ToString());
				this._positions.AddDateTime(DateTime.Now);
				this._positions.AddDateTime(DateTime.Now);
				this._positions.AddDateTime(DateTime.Now);
				this._positions.AddDateTime(DateTime.Now);
				this._positions.AddInt32(null);
				this._positions.AddBoolean(false);
				this._positions.AddBoolean(false);
				this._positions.AddInt32(0);
				this._positions.AddInt32(0);
				this._positions.AddString(this._currentIteration.ToString());
				this._positions.AddString(Guid.NewGuid().ToString());

				this._dedup.NewRecord();
				this._dedup.AddGuid(Guid.NewGuid());
				this._dedup.AddString(Guid.NewGuid().ToString());
				this._dedup.AddString(Guid.NewGuid().ToString());
				this._dedup.AddString(Guid.NewGuid().ToString());

				this._singleOperations++;
			}

			this._savePosition.ExecuteNonQuery();
			this._saveDedup.ExecuteNonQuery();

			sw.Stop();
		}

		public override void Dispose()
		{
			if (this._savePosition != null)
			{
				this._savePosition.Dispose();
				this._savePosition = null;
			}
			if (this._saveDedup != null)
			{
				this._saveDedup.Dispose();
				this._saveDedup = null;
			}
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
