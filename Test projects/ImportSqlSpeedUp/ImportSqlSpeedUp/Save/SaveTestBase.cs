using Maxima;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImportSqlSpeedUp
{
	abstract class SaveTestBase : DbTestBase
	{

		protected SaveTestBase(string name, int iterationCount, string connString) : base(name, iterationCount, connString)
		{
			using (SqlCommand cmd = SqlServer.GetCommand("TRUNCATE TABLE IT_6819804243173122816; TRUNCATE TABLE IT_5278945729007601847;", this._conn))
			{
				cmd.ExecuteNonQuery();
			}
		}
	}
}
