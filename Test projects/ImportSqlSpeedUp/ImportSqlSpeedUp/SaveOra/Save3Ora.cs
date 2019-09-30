using Maxima;
using Microsoft.SqlServer.Server;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportSqlSpeedUp
{
	class Save3Ora : SaveOraTestBase
	{
		int _rowCount;

		OracleCommand _savePosition;
		OracleCommand _saveDedup;
		OracleCommand _updateClob;


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
		OracleParameter _stateTmpParam;
		OracleParameter _stateDateParam;
		OracleParameter _lcDateParam;
		OracleParameter _aaParam;
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

		OracleParameter _nclob01Param;
		OracleParameter _nclobId01Param;
		OracleParameter _nclob02Param;
		OracleParameter _nclobId02Param;
		OracleParameter _nclob03Param;
		OracleParameter _nclobId03Param;
		OracleParameter _nclob04Param;
		OracleParameter _nclobId04Param;
		OracleParameter _nclob05Param;
		OracleParameter _nclobId05Param;
		OracleParameter _nclob06Param;
		OracleParameter _nclobId06Param;
		OracleParameter _nclob07Param;
		OracleParameter _nclobId07Param;
		OracleParameter _nclob08Param;
		OracleParameter _nclobId08Param;
		OracleParameter _nclob09Param;
		OracleParameter _nclobId09Param;
		OracleParameter _nclob10Param;
		OracleParameter _nclobId10Param;

		int _id;

		public Save3Ora(string name, int iterationCount, int rowCount, string connString) : base(name, iterationCount, connString)
		{
			this._rowCount = rowCount;

			this._savePosition = new OracleCommand("IT_5_PKG.array_insert", this._conn);
			this._savePosition.CommandType = CommandType.StoredProcedure;

			this._idParam = new OracleParameter { OracleDbType = OracleDbType.Decimal, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._dirParam = new OracleParameter { OracleDbType = OracleDbType.Decimal, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._guidParam = new OracleParameter { OracleDbType = OracleDbType.Varchar2, Size = 36, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._objectGuidParam = new OracleParameter { OracleDbType = OracleDbType.Varchar2, Size = 36, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._patchParam = new OracleParameter { OracleDbType = OracleDbType.Decimal, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._scParam = new OracleParameter { OracleDbType = OracleDbType.NVarchar2, Size = 370, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._dbdParam = new OracleParameter { OracleDbType = OracleDbType.Date, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._dedParam = new OracleParameter { OracleDbType = OracleDbType.Date, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._abdParam = new OracleParameter { OracleDbType = OracleDbType.Date, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._aedParam = new OracleParameter { OracleDbType = OracleDbType.Date, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._stateParam = new OracleParameter { OracleDbType = OracleDbType.Decimal, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._stateTmpParam = new OracleParameter { OracleDbType = OracleDbType.Decimal, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._stateDateParam = new OracleParameter { OracleDbType = OracleDbType.Date, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._lcDateParam = new OracleParameter { OracleDbType = OracleDbType.Date, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._aaParam = new OracleParameter { OracleDbType = OracleDbType.Decimal, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._rlParam = new OracleParameter { OracleDbType = OracleDbType.Decimal, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._rrParam = new OracleParameter { OracleDbType = OracleDbType.Decimal, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._odParam = new OracleParameter { OracleDbType = OracleDbType.Decimal, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._rtParam = new OracleParameter { OracleDbType = OracleDbType.Decimal, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._c1Param = new OracleParameter { OracleDbType = OracleDbType.NVarchar2, Size = 20, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._c2Param = new OracleParameter { OracleDbType = OracleDbType.NVarchar2, Size = 250, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._c3Param = new OracleParameter { OracleDbType = OracleDbType.Decimal, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._c4Param = new OracleParameter { OracleDbType = OracleDbType.NVarchar2, Size = 10, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._c5Param = new OracleParameter { OracleDbType = OracleDbType.NVarchar2, Size = 17, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._c6Param = new OracleParameter { OracleDbType = OracleDbType.NVarchar2, Size = 6, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._c7Param = new OracleParameter { OracleDbType = OracleDbType.NVarchar2, Size = 4, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._c8Param = new OracleParameter { OracleDbType = OracleDbType.NVarchar2, Size = 4, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._c9Param = new OracleParameter { OracleDbType = OracleDbType.NVarchar2, Size = 11, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._nclobParam = new OracleParameter { OracleDbType = OracleDbType.NVarchar2, Size = 2000, CollectionType = OracleCollectionType.PLSQLAssociativeArray };

			this._savePosition.Parameters.Add(_idParam);
			this._savePosition.Parameters.Add(_dirParam);
			this._savePosition.Parameters.Add(_guidParam);
			this._savePosition.Parameters.Add(_objectGuidParam);
			this._savePosition.Parameters.Add(_patchParam);
			this._savePosition.Parameters.Add(_scParam);
			this._savePosition.Parameters.Add(_dbdParam);
			this._savePosition.Parameters.Add(_dedParam);
			this._savePosition.Parameters.Add(_abdParam);
			this._savePosition.Parameters.Add(_aedParam);
			this._savePosition.Parameters.Add(_stateParam);
			this._savePosition.Parameters.Add(_stateTmpParam);
			this._savePosition.Parameters.Add(_stateDateParam);
			this._savePosition.Parameters.Add(_lcDateParam);
			this._savePosition.Parameters.Add(_aaParam);
			this._savePosition.Parameters.Add(_rlParam);
			this._savePosition.Parameters.Add(_rrParam);
			this._savePosition.Parameters.Add(_odParam);
			this._savePosition.Parameters.Add(_rtParam);
			this._savePosition.Parameters.Add(_c1Param);
			this._savePosition.Parameters.Add(_c2Param);
			this._savePosition.Parameters.Add(_c3Param);
			this._savePosition.Parameters.Add(_c4Param);
			this._savePosition.Parameters.Add(_c5Param);
			this._savePosition.Parameters.Add(_c6Param);
			this._savePosition.Parameters.Add(_c7Param);
			this._savePosition.Parameters.Add(_c8Param);
			this._savePosition.Parameters.Add(_c9Param);
			this._savePosition.Parameters.Add(this._nclobParam);

			this._saveDedup = new OracleCommand("IT_5_PKG.array_insert2", this._conn);
			this._saveDedup.CommandType = CommandType.StoredProcedure;

			this._dedupIdParam = new OracleParameter { OracleDbType = OracleDbType.Varchar2, Size = 36, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._dedup1Param = new OracleParameter { OracleDbType = OracleDbType.NVarchar2, Size = 2000, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._dedup2Param = new OracleParameter { OracleDbType = OracleDbType.NVarchar2, Size = 2000, CollectionType = OracleCollectionType.PLSQLAssociativeArray };
			this._dedup3Param = new OracleParameter { OracleDbType = OracleDbType.NVarchar2, Size = 2000, CollectionType = OracleCollectionType.PLSQLAssociativeArray };

			this._saveDedup.Parameters.Add(_dedupIdParam);
			this._saveDedup.Parameters.Add(_dedup1Param);
			this._saveDedup.Parameters.Add(_dedup2Param);
			this._saveDedup.Parameters.Add(_dedup3Param);

			this._updateClob = new OracleCommand("IT_5_PKG.update_nclob", this._conn);
			this._updateClob.CommandType = CommandType.StoredProcedure;

			this._nclob01Param = new OracleParameter { OracleDbType = OracleDbType.NClob };
			this._nclobId01Param = new OracleParameter { OracleDbType = OracleDbType.Decimal };
			this._nclob02Param = new OracleParameter { OracleDbType = OracleDbType.NClob };
			this._nclobId02Param = new OracleParameter { OracleDbType = OracleDbType.Decimal };
			this._nclob03Param = new OracleParameter { OracleDbType = OracleDbType.NClob };
			this._nclobId03Param = new OracleParameter { OracleDbType = OracleDbType.Decimal };
			this._nclob04Param = new OracleParameter { OracleDbType = OracleDbType.NClob };
			this._nclobId04Param = new OracleParameter { OracleDbType = OracleDbType.Decimal };
			this._nclob05Param = new OracleParameter { OracleDbType = OracleDbType.NClob };
			this._nclobId05Param = new OracleParameter { OracleDbType = OracleDbType.Decimal };
			this._nclob06Param = new OracleParameter { OracleDbType = OracleDbType.NClob };
			this._nclobId06Param = new OracleParameter { OracleDbType = OracleDbType.Decimal };
			this._nclob07Param = new OracleParameter { OracleDbType = OracleDbType.NClob };
			this._nclobId07Param = new OracleParameter { OracleDbType = OracleDbType.Decimal };
			this._nclob08Param = new OracleParameter { OracleDbType = OracleDbType.NClob };
			this._nclobId08Param = new OracleParameter { OracleDbType = OracleDbType.Decimal };
			this._nclob09Param = new OracleParameter { OracleDbType = OracleDbType.NClob };
			this._nclobId09Param = new OracleParameter { OracleDbType = OracleDbType.Decimal };
			this._nclob10Param = new OracleParameter { OracleDbType = OracleDbType.NClob };
			this._nclobId10Param = new OracleParameter { OracleDbType = OracleDbType.Decimal };

			this._updateClob.Parameters.Add(_nclob01Param);
			this._updateClob.Parameters.Add(_nclobId01Param);
			this._updateClob.Parameters.Add(_nclob02Param);
			this._updateClob.Parameters.Add(_nclobId02Param);
			this._updateClob.Parameters.Add(_nclob03Param);
			this._updateClob.Parameters.Add(_nclobId03Param);
			this._updateClob.Parameters.Add(_nclob04Param);
			this._updateClob.Parameters.Add(_nclobId04Param);
			this._updateClob.Parameters.Add(_nclob05Param);
			this._updateClob.Parameters.Add(_nclobId05Param);
			this._updateClob.Parameters.Add(_nclob06Param);
			this._updateClob.Parameters.Add(_nclobId06Param);
			this._updateClob.Parameters.Add(_nclob07Param);
			this._updateClob.Parameters.Add(_nclobId07Param);
			this._updateClob.Parameters.Add(_nclob08Param);
			this._updateClob.Parameters.Add(_nclobId08Param);
			this._updateClob.Parameters.Add(_nclob09Param);
			this._updateClob.Parameters.Add(_nclobId09Param);
			this._updateClob.Parameters.Add(_nclob10Param);
			this._updateClob.Parameters.Add(_nclobId10Param);
		}

		protected override void RunIteration(Stopwatch sw)
		{
			sw.Start();

			List<decimal> idList = new List<decimal>(_rowCount);
			List<decimal> dirList = new List<decimal>(_rowCount);
			List<string> guidList = new List<string>(_rowCount);
			List<string> objectGuidList = new List<string>(_rowCount);
			List<decimal> patchList = new List<decimal>(_rowCount);
			List<string> scList = new List<string>(_rowCount);
			List<DateTime> dbdList = new List<DateTime>(_rowCount);
			List<DateTime> dedList = new List<DateTime>(_rowCount);
			List<DateTime> abdList = new List<DateTime>(_rowCount);
			List<DateTime> aedList = new List<DateTime>(_rowCount);
			List<decimal> stateList = new List<decimal>(_rowCount);
			List<decimal> stateTmpList = new List<decimal>(_rowCount);
			List<DateTime> stateDateList = new List<DateTime>(_rowCount);
			List<DateTime> lcDateList = new List<DateTime>(_rowCount);
			List<decimal> aaList = new List<decimal>(_rowCount);
			List<decimal> rlList = new List<decimal>(_rowCount);
			List<decimal> rrList = new List<decimal>(_rowCount);
			List<decimal> odList = new List<decimal>(_rowCount);
			List<decimal> rtList = new List<decimal>(_rowCount);
			List<string> c1List = new List<string>(_rowCount);
			List<string> c2List = new List<string>(_rowCount);
			List<decimal> c3List = new List<decimal>(_rowCount);
			List<string> c4List = new List<string>(_rowCount);
			List<string> c5List = new List<string>(_rowCount);
			List<string> c6List = new List<string>(_rowCount);
			List<string> c7List = new List<string>(_rowCount);
			List<string> c8List = new List<string>(_rowCount);
			List<string> c9List = new List<string>(_rowCount);
			List<string> nclobList = new List<string>(_rowCount);

			List<int> bsList01 = new List<int>(this._rowCount);
			List<int> bsList02 = new List<int>(this._rowCount);
			List<int> bsList03 = new List<int>(this._rowCount);
			List<int> bsList04 = new List<int>(this._rowCount);
			List<int> bsList05 = new List<int>(this._rowCount);
			List<int> bsList06 = new List<int>(this._rowCount);
			List<int> bsList07 = new List<int>(this._rowCount);
			List<int> bsList08 = new List<int>(this._rowCount);
			List<int> bsList09 = new List<int>(this._rowCount);
			List<int> bsList10 = new List<int>(this._rowCount);
			List<int> bsList11 = new List<int>(this._rowCount);

			List<OracleParameterStatus> absList01 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList02 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList03 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList04 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList05 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList06 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList07 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList08 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList09 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList10 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList11 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList12 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList13 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList14 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList15 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList16 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList17 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList18 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList19 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList20 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList21 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList22 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList23 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList24 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList25 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList26 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList27 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList28 = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> absList29 = new List<OracleParameterStatus>(this._rowCount);



			List<string> dedupIdValueList = new List<string>(this._rowCount);
			List<string> dedup1ValueList = new List<string>(this._rowCount);
			List<string> dedup2ValueList = new List<string>(this._rowCount);
			List<string> dedup3ValueList = new List<string>(this._rowCount);

			List<int> dedupIdBsList = new List<int>(this._rowCount);
			List<int> dedup1BsList = new List<int>(this._rowCount);
			List<int> dedup2BsList = new List<int>(this._rowCount);
			List<int> dedup3BsList = new List<int>(this._rowCount);

			List<OracleParameterStatus> dedupIdAbsList = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> dedup1AbsList = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> dedup2AbsList = new List<OracleParameterStatus>(this._rowCount);
			List<OracleParameterStatus> dedup3AbsList = new List<OracleParameterStatus>(this._rowCount);



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
				stateList.Add(0);
				stateTmpList.Add(0);
				stateDateList.Add(DateTime.Now);
				lcDateList.Add(DateTime.Now);
				aaList.Add(0);
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

				absList01.Add(OracleParameterStatus.Success);
				absList02.Add(OracleParameterStatus.Success);
				absList03.Add(OracleParameterStatus.Success);
				absList04.Add(OracleParameterStatus.Success);
				absList05.Add(OracleParameterStatus.Success);
				absList06.Add(OracleParameterStatus.Success);
				absList07.Add(OracleParameterStatus.Success);
				absList08.Add(OracleParameterStatus.Success);
				absList09.Add(OracleParameterStatus.Success);
				absList10.Add(OracleParameterStatus.Success);
				absList11.Add(OracleParameterStatus.NullInsert);
				absList12.Add(OracleParameterStatus.NullInsert);
				absList13.Add(OracleParameterStatus.NullInsert);
				absList14.Add(OracleParameterStatus.NullInsert);
				absList15.Add(OracleParameterStatus.Success);
				absList16.Add(OracleParameterStatus.Success);
				absList17.Add(OracleParameterStatus.Success);
				absList18.Add(OracleParameterStatus.Success);
				absList19.Add(OracleParameterStatus.Success);
				absList20.Add(OracleParameterStatus.Success);
				absList21.Add(OracleParameterStatus.Success);
				absList22.Add(OracleParameterStatus.Success);
				absList23.Add(OracleParameterStatus.Success);
				absList24.Add(OracleParameterStatus.Success);
				absList25.Add(OracleParameterStatus.Success);
				absList26.Add(OracleParameterStatus.Success);
				absList27.Add(OracleParameterStatus.Success);
				absList28.Add(OracleParameterStatus.Success);
				absList29.Add(OracleParameterStatus.Success);

				bsList01.Add(36);
				bsList02.Add(36);
				bsList03.Add(36);
				bsList04.Add(this._currentIteration.ToString().Length);
				bsList05.Add(36);
				bsList06.Add(this._currentIteration.ToString().Length);
				bsList07.Add(this._currentIteration.ToString().Length);
				bsList08.Add(this._currentIteration.ToString().Length);
				bsList09.Add((this._currentIteration / 100).ToString().Length);
				bsList10.Add((this._currentIteration / 100).ToString().Length);
				bsList11.Add(this._currentIteration.ToString().Length);



				dedupIdValueList.Add(Guid.NewGuid().ToString());
				dedup1ValueList.Add(Guid.NewGuid().ToString());
				dedup2ValueList.Add(Guid.NewGuid().ToString());
				dedup3ValueList.Add(Guid.NewGuid().ToString());

				dedupIdBsList.Add(36);
				dedup1BsList.Add(36);
				dedup2BsList.Add(36);
				dedup3BsList.Add(36);

				dedupIdAbsList.Add(OracleParameterStatus.Success);
				dedup1AbsList.Add(i % 3 == 0 ? OracleParameterStatus.NullInsert : OracleParameterStatus.Success);
				dedup2AbsList.Add(i % 3 == 1 ? OracleParameterStatus.NullInsert : OracleParameterStatus.Success);
				dedup3AbsList.Add(i % 3 == 2 ? OracleParameterStatus.NullInsert : OracleParameterStatus.Success);
			}


			this.SetParamValue(this._idParam, idList.ToArray(), null, absList01, this._rowCount);
			this.SetParamValue(this._dirParam, dirList.ToArray(), null, absList02, this._rowCount);
			this.SetParamValue(this._guidParam, guidList.ToArray(), bsList01, absList03, this._rowCount);
			this.SetParamValue(this._objectGuidParam, objectGuidList.ToArray(), bsList02, absList04, this._rowCount);
			this.SetParamValue(this._patchParam, patchList.ToArray(), null, absList05, this._rowCount);
			this.SetParamValue(this._scParam, scList.ToArray(), bsList03, absList06, this._rowCount);
			this.SetParamValue(this._dbdParam, dbdList.ToArray(), null, absList07, this._rowCount);
			this.SetParamValue(this._dedParam, dedList.ToArray(), null, absList08, this._rowCount);
			this.SetParamValue(this._abdParam, abdList.ToArray(), null, absList09, this._rowCount);
			this.SetParamValue(this._aedParam, aedList.ToArray(), null, absList10, this._rowCount);
			this.SetParamValue(this._stateParam, stateList.ToArray(), null, absList11, this._rowCount);
			this.SetParamValue(this._stateTmpParam, stateTmpList.ToArray(), null, absList12, this._rowCount);
			this.SetParamValue(this._stateDateParam, stateDateList.ToArray(), null, absList13, this._rowCount);
			this.SetParamValue(this._lcDateParam, lcDateList.ToArray(), null, absList14, this._rowCount);
			this.SetParamValue(this._aaParam, aaList.ToArray(), null, absList15, this._rowCount);
			this.SetParamValue(this._rlParam, rlList.ToArray(), null, absList16, this._rowCount);
			this.SetParamValue(this._rrParam, rrList.ToArray(), null, absList17, this._rowCount);
			this.SetParamValue(this._odParam, odList.ToArray(), null, absList18, this._rowCount);
			this.SetParamValue(this._rtParam, rtList.ToArray(), null, absList19, this._rowCount);
			this.SetParamValue(this._c1Param, c1List.ToArray(), bsList04, absList20, this._rowCount);
			this.SetParamValue(this._c2Param, c2List.ToArray(), bsList05, absList21, this._rowCount);
			this.SetParamValue(this._c3Param, c3List.ToArray(), null, absList22, this._rowCount);
			this.SetParamValue(this._c4Param, c4List.ToArray(), bsList06, absList23, this._rowCount);
			this.SetParamValue(this._c5Param, c5List.ToArray(), bsList07, absList24, this._rowCount);
			this.SetParamValue(this._c6Param, c6List.ToArray(), bsList08, absList25, this._rowCount);
			this.SetParamValue(this._c7Param, c7List.ToArray(), bsList09, absList26, this._rowCount);
			this.SetParamValue(this._c8Param, c8List.ToArray(), bsList10, absList27, this._rowCount);
			this.SetParamValue(this._c9Param, c9List.ToArray(), bsList11, absList28, this._rowCount);
			this.SetParamValue(this._nclobParam, nclobList.ToArray(), null, absList29, this._rowCount);


			this.SetParamValue(this._dedupIdParam, dedupIdValueList.ToArray(), dedupIdBsList, dedupIdAbsList, this._rowCount);
			this.SetParamValue(this._dedup1Param, dedup1ValueList.ToArray(), dedup1BsList, dedup1AbsList, this._rowCount);
			this.SetParamValue(this._dedup2Param, dedup2ValueList.ToArray(), dedup2BsList, dedup2AbsList, this._rowCount);
			this.SetParamValue(this._dedup3Param, dedup3ValueList.ToArray(), dedup3BsList, dedup3AbsList, this._rowCount);


			using (OracleTransaction tx = this._conn.BeginTransaction())
			{
				this._savePosition.Transaction = tx;
				this._savePosition.ExecuteNonQuery();
				//this._saveDedup.ExecuteNonQuery();
				tx.Commit();
			}


			//using (OracleTransaction tx = this._conn.BeginTransaction())
			//{
			//	this._updateClob.Transaction = tx;
			//	for (int i = 0; i < _rowCount / 10; i++)
			//	{
			//		int id = this._id - this._rowCount + i * 10;
			//		this._nclob01Param.Value = Guid.NewGuid().ToString();
			//		this._nclobId01Param.Value = id + 1;
			//		this._nclob02Param.Value = Guid.NewGuid().ToString();
			//		this._nclobId02Param.Value = id + 2;
			//		this._nclob03Param.Value = Guid.NewGuid().ToString();
			//		this._nclobId03Param.Value = id + 3;
			//		this._nclob04Param.Value = Guid.NewGuid().ToString();
			//		this._nclobId04Param.Value = id + 4;
			//		this._nclob05Param.Value = Guid.NewGuid().ToString();
			//		this._nclobId05Param.Value = id + 5;
			//		this._nclob06Param.Value = Guid.NewGuid().ToString();
			//		this._nclobId06Param.Value = id + 6;
			//		this._nclob07Param.Value = Guid.NewGuid().ToString();
			//		this._nclobId07Param.Value = id + 7;
			//		this._nclob08Param.Value = Guid.NewGuid().ToString();
			//		this._nclobId08Param.Value = id + 8;
			//		this._nclob09Param.Value = Guid.NewGuid().ToString();
			//		this._nclobId09Param.Value = id + 9;
			//		this._nclob10Param.Value = Guid.NewGuid().ToString();
			//		this._nclobId10Param.Value = id + 10;
			//		this._updateClob.ExecuteNonQuery();
			//	}
			//	tx.Commit();
			//}

			sw.Stop();
		}

		private void SetParamValue(OracleParameter param, Array value, List<int> bsList, List<OracleParameterStatus> absList, int rowCount)
		{
			param.Value = value;
			if (bsList != null)
			{
				param.ArrayBindSize = bsList.ToArray();
			}

			param.ArrayBindStatus = absList.ToArray();
			param.Size = this._rowCount;
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
