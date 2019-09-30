using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportSqlSpeedUp.SaveBlobOra
{
	class SaveNclobOra1 : SaveOraTestBase
	{
		int _rowCount;

		OracleCommand _savePosition;

		OracleParameter _idParam;
		OracleParameter _nclobParam;

		string _nclob;

		int _id;

		public SaveNclobOra1(string name, int iterationCount, int rowCount, string connString) : base(name, iterationCount, connString)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < 32000; i++)
			{
				sb.Append(Guid.NewGuid().ToString());
			}
			_nclob = sb.ToString();

			this._rowCount = rowCount;

			this._savePosition = new OracleCommand($@"INSERT /*+ append */ INTO c(ID, D) VALUES (:ID, :D)", this._conn);
			this._idParam = new OracleParameter(":ID", OracleDbType.Decimal);
			this._nclobParam = new OracleParameter(":D", OracleDbType.NClob);

			this._savePosition.Parameters.Add(this._idParam);
			this._savePosition.Parameters.Add(this._nclobParam);
		}

		protected override void RunIteration(Stopwatch sw)
		{
			this.TruncateTables();
			sw.Start();

			List<decimal> idList = new List<decimal>(this._rowCount);
			List<string> blobList = new List<string>(this._rowCount);

			for (int i = 0; i < this._rowCount; i++)
			{
				idList.Add(++this._id);
				blobList.Add(this._nclob);
			}

			_idParam.Value = idList.ToArray();
			_nclobParam.Value = blobList.ToArray();

			this._savePosition.ArrayBindCount = this._rowCount;

			this._savePosition.ExecuteNonQuery();

			sw.Stop();
		}

		public override void Dispose()
		{
			if (this._savePosition != null)
			{
				this._savePosition.Dispose();
				this._savePosition = null;
			}
			base.Dispose();
		}
	}
}
