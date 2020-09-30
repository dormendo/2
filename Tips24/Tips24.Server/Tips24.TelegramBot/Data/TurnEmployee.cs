using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.TelegramBot
{
	public class TurnEmployee
	{
		public int EmployeeId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public int GroupId { get; set; }
		public string GroupName { get; set; }
		public bool IsInTurn { get; set; }

		public TurnEmployee(SqlDataReader dr)
		{
			this.EmployeeId = dr.GetInt32("EmployeeId");
			this.FirstName = dr.GetString("FirstName");
			this.LastName = dr.GetString("LastName");
			this.GroupId = dr.GetInt32("GroupId");
			this.GroupName = dr.GetString("GroupName");
			this.IsInTurn = (dr.GetInt32("IsInTurn") != 0);
		}
	}
}
