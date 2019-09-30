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
	class SaveBlobOra1 : SaveOraTestBase
	{
		int _rowCount;

		OracleCommand _savePosition;

		OracleParameter _idParam;
		OracleParameter _blobParam;

		byte[] _blob;

		int _id;

		public SaveBlobOra1(string name, int iterationCount, int rowCount, string connString) : base(name, iterationCount, connString)
		{
			_blob = File.ReadAllBytes("C:\\temp\\files\\1.rar");

			this._rowCount = rowCount;

			this._savePosition = new OracleCommand($@"INSERT /*+ append */ INTO B(ID, D) VALUES (:ID, :D)", this._conn);
			this._idParam = new OracleParameter(":ID", OracleDbType.Decimal);
			this._blobParam = new OracleParameter(":D", OracleDbType.Blob);

			this._savePosition.Parameters.Add(this._idParam);
			this._savePosition.Parameters.Add(this._blobParam);
		}

		protected override void RunIteration(Stopwatch sw)
		{
			this.TruncateTables();
			sw.Start();

			List<decimal> idList = new List<decimal>(this._rowCount);
			List<byte[]> blobList = new List<byte[]>(this._rowCount);

			for (int i = 0; i < this._rowCount; i++)
			{
				idList.Add(++this._id);
				blobList.Add(this._blob);
			}

			_idParam.Value = idList.ToArray();
			_blobParam.Value = blobList.ToArray();

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
