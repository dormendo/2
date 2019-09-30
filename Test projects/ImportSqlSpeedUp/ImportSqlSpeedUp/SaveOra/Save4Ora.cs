using Maxima;
using Microsoft.SqlServer.Server;
using Oracle.ManagedDataAccess.Client;
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
	class Save4Ora : SaveOraTestBase
	{
		int _rowCount;

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

		int _id;

		public Save4Ora(string name, int iterationCount, int rowCount, string connString) : base(name, iterationCount, connString)
		{
			this._rowCount = rowCount;

			this._savePosition = new OracleCommand(
				$@"INSERT /*+ append */ INTO IT_5(ID, DIRECTIVE, GUID, OBJECTID, PATCHID, USERCODE, DOCUMENTBEGINDATE, DOCUMENTENDDATE, ACTUALBEGINDATE, ACTUALENDDATE, STATE, REMOVELEFT, REMOVERIGHT,
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
			this._nclobParam = new OracleParameter(":c_nclob", OracleDbType.NVarchar2, 2000);

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

			this._saveDedup = new OracleCommand("INSERT /*+ append */ INTO IT_6(ID, DEDUP_STR_1, DEDUP_STR_2, DEDUP_STR_3) VALUES (:inserted_id, :DEDUP_STR_1, :DEDUP_STR_2, :DEDUP_STR_3)", this._conn);
			this._dedupIdParam = new OracleParameter(":inserted_id", OracleDbType.Varchar2, 36);
			this._dedup1Param = new OracleParameter(":DEDUP_STR_1", OracleDbType.NVarchar2, 2000);
			this._dedup2Param = new OracleParameter(":DEDUP_STR_2", OracleDbType.NVarchar2, 2000);
			this._dedup3Param = new OracleParameter(":DEDUP_STR_3", OracleDbType.NVarchar2, 2000);

			this._saveDedup.Parameters.Add(this._dedupIdParam);
			this._saveDedup.Parameters.Add(this._dedup1Param);
			this._saveDedup.Parameters.Add(this._dedup2Param);
			this._saveDedup.Parameters.Add(this._dedup3Param);
		}

		protected override void RunIteration(Stopwatch sw)
		{
			sw.Start();

			List<decimal> idList = new List<decimal>(this._rowCount);
			List<decimal> dirList = new List<decimal>(this._rowCount);
			List<string> guidList = new List<string>(this._rowCount);
			List<string> objectGuidList = new List<string>(this._rowCount);
			List<decimal> patchList = new List<decimal>(this._rowCount);
			List<string> scList = new List<string>(this._rowCount);
			List<DateTime> dbdList = new List<DateTime>(this._rowCount);
			List<DateTime> dedList = new List<DateTime>(this._rowCount);
			List<DateTime> abdList = new List<DateTime>(this._rowCount);
			List<DateTime> aedList = new List<DateTime>(this._rowCount);
			List<object> stateList = new List<object>(this._rowCount);
			List<decimal> rlList = new List<decimal>(this._rowCount);
			List<decimal> rrList = new List<decimal>(this._rowCount);
			List<decimal> odList = new List<decimal>(this._rowCount);
			List<decimal> rtList = new List<decimal>(this._rowCount);
			List<string> c1List = new List<string>(this._rowCount);
			List<string> c2List = new List<string>(this._rowCount);
			List<decimal> c3List = new List<decimal>(this._rowCount);
			List<string> c4List = new List<string>(this._rowCount);
			List<string> c5List = new List<string>(this._rowCount);
			List<string> c6List = new List<string>(this._rowCount);
			List<string> c7List = new List<string>(this._rowCount);
			List<string> c8List = new List<string>(this._rowCount);
			List<string> c9List = new List<string>(this._rowCount);
			List<string> nclobList = new List<string>(this._rowCount);

			List<string> dedupIdList = new List<string>(this._rowCount);
			List<string> dedup1List = new List<string>(this._rowCount);
			List<string> dedup2List = new List<string>(this._rowCount);
			List<string> dedup3List = new List<string>(this._rowCount);

			for (int i = 0; i < this._rowCount; i++)
			{
				idList.Add(++this._id);
				dirList.Add(3);
				guidList.Add(Guid.NewGuid().ToString());
				objectGuidList.Add(Guid.NewGuid().ToString());
				patchList.Add(3);
				scList.Add(this._currentIteration.ToString());
				dbdList.Add(DateTime.Now);
				dedList.Add(DateTime.Now);
				abdList.Add(DateTime.Now);
				aedList.Add(DateTime.Now);
				stateList.Add(DBNull.Value);
				rlList.Add(0);
				rrList.Add(0);
				odList.Add(0);
				rtList.Add(0);
				c1List.Add(this._currentIteration.ToString());
				c2List.Add(Guid.NewGuid().ToString());
				c3List.Add(this._currentIteration);
				c4List.Add(this._currentIteration.ToString());
				c5List.Add(this._currentIteration.ToString());
				c6List.Add(this._currentIteration.ToString());
				c7List.Add((this._currentIteration / 100).ToString());
				c8List.Add((this._currentIteration / 100).ToString());
				c9List.Add(this._currentIteration.ToString());
				nclobList.Add(Guid.NewGuid().ToString());

				dedupIdList.Add(Guid.NewGuid().ToString());
				dedup1List.Add(Guid.NewGuid().ToString());
				dedup2List.Add(Guid.NewGuid().ToString());
				dedup3List.Add(Guid.NewGuid().ToString());
			}

			_idParam.Value = idList.ToArray();
			_dirParam.Value = dirList.ToArray();
			_guidParam.Value = guidList.ToArray();
			_objectGuidParam.Value = objectGuidList.ToArray();
			_patchParam.Value = patchList.ToArray();
			_scParam.Value = scList.ToArray();
			_dbdParam.Value = dbdList.ToArray();
			_dedParam.Value = dedList.ToArray();
			_abdParam.Value = abdList.ToArray();
			_aedParam.Value = aedList.ToArray();
			_stateParam.Value = stateList.ToArray();
			_rlParam.Value = rlList.ToArray();
			_rrParam.Value = rrList.ToArray();
			_odParam.Value = odList.ToArray();
			_rtParam.Value = rtList.ToArray();
			_c1Param.Value = c1List.ToArray();
			_c2Param.Value = c2List.ToArray();
			_c3Param.Value = c3List.ToArray();
			_c4Param.Value = c4List.ToArray();
			_c5Param.Value = c5List.ToArray();
			_c6Param.Value = c6List.ToArray();
			_c7Param.Value = c7List.ToArray();
			_c8Param.Value = c8List.ToArray();
			_c9Param.Value = c9List.ToArray();
			_nclobParam.Value = c9List.ToArray();

			_dedupIdParam.Value = dedupIdList.ToArray();
			_dedup1Param.Value = dedup1List.ToArray();
			_dedup2Param.Value = dedup2List.ToArray();
			_dedup3Param.Value = dedup3List.ToArray();

			this._savePosition.ArrayBindCount = this._rowCount;
			this._saveDedup.ArrayBindCount = this._rowCount;

			using (OracleTransaction tx = this._conn.BeginTransaction())
			{
				this._savePosition.Transaction = tx;
				this._saveDedup.Transaction = tx;
				this._savePosition.ExecuteNonQuery();
				//this._saveDedup.ExecuteNonQuery();
				tx.Commit();
			}

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
