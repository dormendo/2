﻿using Maxima;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportSqlSpeedUp
{
	class SaveOriginal : SaveTestBase
	{
		SqlCommand _savePosition;
		SqlCommand _saveDedup;

		SqlParameter _dirParam;
		SqlParameter _guidParam;
		SqlParameter _objectGuidParam;
		SqlParameter _patchParam;
		SqlParameter _scParam;
		SqlParameter _dbdParam;
		SqlParameter _dedParam;
		SqlParameter _abdParam;
		SqlParameter _aedParam;
		SqlParameter _stateParam;
		SqlParameter _rlParam;
		SqlParameter _rrParam;
		SqlParameter _odParam;
		SqlParameter _rtParam;
		SqlParameter _c1Param;
		SqlParameter _c2Param;

		SqlParameter _dedupIdParam;
		SqlParameter _dedup1Param;
		SqlParameter _dedup2Param;
		SqlParameter _dedup3Param;

		public SaveOriginal(string name, int iterationCount, string connString, bool prepareCommands) : base(name, iterationCount, connString)
		{
			using (SqlCommand cmd = SqlServer.GetCommand("TRUNCATE TABLE IT_6819804243173122816; TRUNCATE TABLE IT_5278945729007601847;", this._conn))
			{
				cmd.ExecuteNonQuery();
			}

			this._savePosition = SqlServer.GetCommand("INSERT INTO IT_6819804243173122816(Directive, GUID, ObjectID, PatchID, UserCode, DocumentBeginDate, DocumentEndDate, ActualBeginDate, ActualEndDate, State, RemoveLeft, RemoveRight, ObjectDel, RecordType, C1783376938461729511, C8592504141572910447) OUTPUT inserted.ID VALUES (@Dir, @GUID, @ObjectGUID, @PatchID, @SC, @DocumentBeginDate, @DocumentEndDate, @ActualBeginDate, @ActualEndDate, @State, @RemoveLeft, @RemoveRight, @ObjectDel, @RecordType, @C1, @C2);", this._conn);
			this._dirParam = this._savePosition.AddIntParam("@Dir");
			this._guidParam = this._savePosition.AddUniqueIdentifierParam("@GUID");
			this._objectGuidParam = this._savePosition.AddUniqueIdentifierParam("@ObjectGUID");
			this._patchParam = this._savePosition.AddIntParam("@PatchID");
			this._scParam = this._savePosition.AddNVarCharParam("@SC", 1024);
			this._dbdParam = this._savePosition.AddDateTimeParam("@DocumentBeginDate");
			this._dedParam = this._savePosition.AddDateTimeParam("@DocumentEndDate");
			this._abdParam = this._savePosition.AddDateTimeParam("@ActualBeginDate");
			this._aedParam = this._savePosition.AddDateTimeParam("@ActualEndDate");
			this._stateParam = this._savePosition.AddNVarCharParam("@State", 4000);
			this._rlParam = this._savePosition.AddBitParam("@RemoveLeft");
			this._rrParam = this._savePosition.AddBitParam("@RemoveRight");
			this._odParam = this._savePosition.AddIntParam("@ObjectDel");
			this._rtParam = this._savePosition.AddIntParam("@RecordType");
			this._c1Param = this._savePosition.AddNVarCharParam("@C1", 370);
			this._c2Param = this._savePosition.AddNVarCharParam("@C2", 4000);

			this._saveDedup = SqlServer.GetCommand("INSERT INTO IT_5278945729007601847(ID, DEDUP_STR_1, DEDUP_STR_2, DEDUP_STR_3) VALUES (@inserted_id, @DEDUP_STR_1, @DEDUP_STR_2, @DEDUP_STR_3)", this._conn);
			this._dedupIdParam = this._saveDedup.AddUniqueIdentifierParam("@inserted_id");
			this._dedup1Param = this._saveDedup.AddNVarCharParam("@DEDUP_STR_1", 4000);
			this._dedup2Param = this._saveDedup.AddNVarCharParam("@DEDUP_STR_2", 4000);
			this._dedup3Param = this._saveDedup.AddNVarCharParam("@DEDUP_STR_3", 4000);

			if (prepareCommands)
			{
				this._savePosition.Prepare();
				this._saveDedup.Prepare();
			}
		}

		protected override void RunIteration(Stopwatch sw)
		{
			sw.Start();

			this._dirParam.Value = 3;
			this._guidParam.Value = Guid.NewGuid();
			this._objectGuidParam.Value = Guid.NewGuid();
			this._patchParam.Value = 3;
			this._scParam.Value = this._currentIteration.ToString();
			this._dbdParam.Value = DateTime.Now;
			this._dedParam.Value = DateTime.Now;
			this._abdParam.Value = DateTime.Now;
			this._aedParam.Value = DateTime.Now;
			this._stateParam.Value = DBNull.Value;
			this._rlParam.Value = false;
			this._rrParam.Value = false;
			this._odParam.Value = 0;
			this._rtParam.Value = 0;
			this._c1Param.Value = this._currentIteration.ToString();
			this._c2Param.Value = Guid.NewGuid().ToString();

			this._dedupIdParam.Value = Guid.NewGuid();
			this._dedup1Param.Value = Guid.NewGuid().ToString();
			this._dedup2Param.Value = Guid.NewGuid().ToString();
			this._dedup3Param.Value = Guid.NewGuid().ToString();

			this._savePosition.ExecuteScalar();
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
			base.Dispose();
		}
	}
}
