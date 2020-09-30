using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.Dto.Auth
{
	public class CheckStatusResponse
	{
		public int EmployeeId { get; set; }
		public string EmployeeFirstName { get; set; }
		public string EmployeeLastName { get; set; }
		public bool EmployeeIsDisabled { get; set; }
		public int? PlaceGroupId { get; set; }
		public string PlaceGroupName { get; set; }
	}
}
