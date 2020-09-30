using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.Dto.Auth
{
	public class FollowReglinkResponse
	{
		public int LinkPlaceId { get; set; }

		public string LinkPlaceName { get; set; }

		public string LinkPlaceAddress { get; set; }

		public string LinkPlaceCity { get; set; }

		public int? EmployeeId { get; set; }

		public int? EmployeePlaceId { get; set; }

		public bool? EmployeeIsDisabled { get; set; }
	}
}
