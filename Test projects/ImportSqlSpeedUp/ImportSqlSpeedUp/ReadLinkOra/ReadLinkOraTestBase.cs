using Maxima;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportSqlSpeedUp
{
	abstract class ReadLinkOraTestBase : DbOraTestBase
	{
		private static List<string> _idList;

		private int _rowCount;

		public ReadLinkOraTestBase(string name, int iterationCount,  int rowCount, string connString)
			: base(name, iterationCount, connString)
		{
			if (_idList == null)
			{
				_idList = new List<string>(50000);
				using (OracleCommand cmd = new OracleCommand("SELECT UserCode FROM (SELECT UserCode FROM E52190 WHERE STATE = 0 AND TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'YYYY-MM-dd HH24:MI:SS') BETWEEN DocumentBeginDate AND DocumentEndDate ORDER BY UserCode) a where rownum < 50001", this._conn))
				{
					using (OracleDataReader dr = cmd.ExecuteReader())
					{
						while (dr.Read())
						{
							_idList.Add(dr.GetString(0));
						}
					}
				}
			}

			this._rowCount = rowCount;
			if (this._rowCount > _idList.Count)
			{
				this._rowCount = _idList.Count;
			}
		}

		protected List<string> GetSet()
		{
			HashSet<int> indexes = new HashSet<int>();
			Random rnd = new Random(DateTime.Now.Second * DateTime.Now.Millisecond);
			while (indexes.Count < this._rowCount)
			{
				int index = rnd.Next(0, _idList.Count);
				indexes.Add(index);
			}

			List<string> result = new List<string>(this._rowCount);
			foreach (int index in indexes)
			{
				result.Add(_idList[index]);
			}
			return result;
		}
	}
}
