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
	class SaveOriginalOra : SaveOraTestBase
	{
		OracleCommand _savePosition;
		OracleCommand _saveDedup;

		OracleParameter _idParam;
		OracleParameter _dirParam;
		OracleParameter _guidParam;
		OracleParameter _objectGuidParam;
		OracleParameter _patchParam;
		OracleParameter _scParam;
		OracleParameter _dbdParam;
		OracleParameter _dedParam;
		OracleParameter _abdParam;
		OracleParameter _aedParam;
		OracleParameter _stateParam;
		OracleParameter _rlParam;
		OracleParameter _rrParam;
		OracleParameter _odParam;
		OracleParameter _rtParam;
		OracleParameter _c1Param;
		OracleParameter _c2Param;
		OracleParameter _c3Param;
		OracleParameter _c4Param;
		OracleParameter _c5Param;
		OracleParameter _c6Param;
		OracleParameter _c7Param;
		OracleParameter _c8Param;
		OracleParameter _c9Param;
		OracleParameter _nclobParam;

		OracleParameter _dedupIdParam;
		OracleParameter _dedup1Param;
		OracleParameter _dedup2Param;
		OracleParameter _dedup3Param;

		int _id = 0;

		public SaveOriginalOra(string name, int iterationCount, string connString, bool prepareCommands) : base(name, iterationCount, connString)
		{
			this._savePosition = new OracleCommand(
				$@"INSERT INTO IT_5(ID, DIRECTIVE, GUID, OBJECTID, PATCHID, USERCODE, DOCUMENTBEGINDATE, DOCUMENTENDDATE, ACTUALBEGINDATE, ACTUALENDDATE, STATE, REMOVELEFT, REMOVERIGHT,
					OBJECTDEL, RECORDTYPE, C1783376938461729511, C8592504141572910447, C2089172047110907212, C5269410614712304966, C1095156487921478956, C4437532284984408449, C2931805783443429356, C1245021195155043293, C7241011763022076318, C_NCLOB)
				VALUES
					(:Id, :Dir, :GUID, :ObjectGUID, :PatchID, :SC, :DocumentBeginDate, :DocumentEndDate, :ActualBeginDate, :ActualEndDate, :State, :RemoveLeft, :RemoveRight,
					:ObjectDel, :RecordType, :C1, :C2, :C3, :C4, :C5, :C6, :C7, :C8, :C9, :c_nclob)", this._conn);
			this._idParam = new OracleParameter(":Id", OracleDbType.Decimal);
			this._dirParam = new OracleParameter(":Dir", OracleDbType.Decimal);
			this._guidParam = new OracleParameter(":GUID", OracleDbType.Varchar2, 36);
			this._objectGuidParam = new OracleParameter(":ObjectGUID", OracleDbType.Varchar2, 36);
			this._patchParam = new OracleParameter(":PatchID", OracleDbType.Decimal);
			this._scParam = new OracleParameter(":SC", OracleDbType.NVarchar2, 370);
			this._dbdParam = new OracleParameter(":DocumentBeginDate", OracleDbType.Date);
			this._dedParam = new OracleParameter(":DocumentEndDate", OracleDbType.Date);
			this._abdParam = new OracleParameter(":ActualBeginDate", OracleDbType.Date);
			this._aedParam = new OracleParameter(":ActualEndDate", OracleDbType.Date);
			this._stateParam = new OracleParameter(":State", OracleDbType.Decimal);
			this._rlParam = new OracleParameter(":RemoveLeft", OracleDbType.Decimal);
			this._rrParam = new OracleParameter(":RemoveRight", OracleDbType.Decimal);
			this._odParam = new OracleParameter(":ObjectDel", OracleDbType.Decimal);
			this._rtParam = new OracleParameter(":RecordType", OracleDbType.Decimal);
			this._c1Param = new OracleParameter(":C1", OracleDbType.NVarchar2, 20);
			this._c2Param = new OracleParameter(":C2", OracleDbType.NVarchar2, 250);
			this._c3Param = new OracleParameter(":C3", OracleDbType.Decimal);
			this._c4Param = new OracleParameter(":C4", OracleDbType.NVarchar2, 10);
			this._c5Param = new OracleParameter(":C5", OracleDbType.NVarchar2, 17);
			this._c6Param = new OracleParameter(":C6", OracleDbType.NVarchar2, 6);
			this._c7Param = new OracleParameter(":C7", OracleDbType.NVarchar2, 4);
			this._c8Param = new OracleParameter(":C8", OracleDbType.NVarchar2, 4);
			this._c9Param = new OracleParameter(":C9", OracleDbType.NVarchar2, 11);
			this._nclobParam = new OracleParameter(":c_nclob", OracleDbType.NClob);

			this._savePosition.Parameters.Add(this._idParam);
			this._savePosition.Parameters.Add(this._dirParam);
			this._savePosition.Parameters.Add(this._guidParam);
			this._savePosition.Parameters.Add(this._objectGuidParam);
			this._savePosition.Parameters.Add(this._patchParam);
			this._savePosition.Parameters.Add(this._scParam);
			this._savePosition.Parameters.Add(this._dbdParam);
			this._savePosition.Parameters.Add(this._dedParam);
			this._savePosition.Parameters.Add(this._abdParam);
			this._savePosition.Parameters.Add(this._aedParam);
			this._savePosition.Parameters.Add(this._stateParam);
			this._savePosition.Parameters.Add(this._rlParam);
			this._savePosition.Parameters.Add(this._rrParam);
			this._savePosition.Parameters.Add(this._odParam);
			this._savePosition.Parameters.Add(this._rtParam);
			this._savePosition.Parameters.Add(this._c1Param);
			this._savePosition.Parameters.Add(this._c2Param);
			this._savePosition.Parameters.Add(this._c3Param);
			this._savePosition.Parameters.Add(this._c4Param);
			this._savePosition.Parameters.Add(this._c5Param);
			this._savePosition.Parameters.Add(this._c6Param);
			this._savePosition.Parameters.Add(this._c7Param);
			this._savePosition.Parameters.Add(this._c8Param);
			this._savePosition.Parameters.Add(this._c9Param);
			this._savePosition.Parameters.Add(this._nclobParam);

			this._saveDedup = new OracleCommand("INSERT INTO IT_6(ID, DEDUP_STR_1, DEDUP_STR_2, DEDUP_STR_3) VALUES (:inserted_id, :DEDUP_STR_1, :DEDUP_STR_2, :DEDUP_STR_3)", this._conn);
			this._dedupIdParam = new OracleParameter(":inserted_id", OracleDbType.Varchar2, 36);
			this._dedup1Param = new OracleParameter(":DEDUP_STR_1", OracleDbType.NClob);
			this._dedup2Param = new OracleParameter(":DEDUP_STR_2", OracleDbType.NClob);
			this._dedup3Param = new OracleParameter(":DEDUP_STR_3", OracleDbType.NClob);

			this._saveDedup.Parameters.Add(this._dedupIdParam);
			this._saveDedup.Parameters.Add(this._dedup1Param);
			this._saveDedup.Parameters.Add(this._dedup2Param);
			this._saveDedup.Parameters.Add(this._dedup3Param);

			if (prepareCommands)
			{
				this._savePosition.Prepare();
				this._saveDedup.Prepare();
			}
		}

		protected override void RunIteration(Stopwatch sw)
		{
			sw.Start();

			this._idParam.Value = ++this._id;
			this._dirParam.Value = 3;
			this._guidParam.Value = Guid.NewGuid().ToString();
			this._objectGuidParam.Value = Guid.NewGuid().ToString();
			this._patchParam.Value = 3;
			this._scParam.Value = this._currentIteration.ToString();
			this._dbdParam.Value = DateTime.Now;
			this._dedParam.Value = DateTime.Now;
			this._abdParam.Value = DateTime.Now;
			this._aedParam.Value = DateTime.Now;
			this._stateParam.Value = DBNull.Value;
			this._rlParam.Value = 0;
			this._rrParam.Value = 0;
			this._odParam.Value = 0;
			this._rtParam.Value = 0;
			this._c1Param.Value = this._currentIteration.ToString();
			this._c2Param.Value = Guid.NewGuid().ToString();
			this._c3Param.Value = this._currentIteration;
			this._c4Param.Value = this._currentIteration.ToString();
			this._c5Param.Value = this._currentIteration.ToString();
			this._c6Param.Value = this._currentIteration.ToString();
			this._c7Param.Value = (this._currentIteration / 100).ToString();
			this._c8Param.Value = (this._currentIteration / 100).ToString();
			this._c9Param.Value = this._currentIteration.ToString();
			this._nclobParam.Value = this._currentIteration.ToString();

			this._dedupIdParam.Value = Guid.NewGuid().ToString();
			this._dedup1Param.Value = Guid.NewGuid().ToString();
			this._dedup2Param.Value = Guid.NewGuid().ToString();
			this._dedup3Param.Value = Guid.NewGuid().ToString();

			this._savePosition.ExecuteScalar();
			//this._saveDedup.ExecuteNonQuery();

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
