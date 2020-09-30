using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.Acquiring.Controllers.PaymentPage
{
	public class SucceedRequestHandler
	{
		private Response _response;
		private SqlServer _sqlServer;

		private int _requestId;

		private PaymentType _type;
		private int _placeId;
		private int? _employeeId;
		private decimal _amount;

		public SucceedRequestHandler(SqlServer sqlServer)
		{
			_sqlServer = sqlServer;
		}

		public async Task<IActionResult> Handle(int requestId)
		{
			_requestId = requestId;
			_response = new Response();

			await this.HandleInternal();

			if (_response.HasError)
			{
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, _response.ErrorMessage, null);
			}
			else
			{
				string msg = _type.ToString() + ", " + _placeId.ToString() + (_employeeId.HasValue ? "=" + _employeeId.Value.ToString() : "") + ", " + _amount.ToString() + " руб.";
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, msg, null);
			}

			return new JsonResult(_response);
		}

		private async Task HandleInternal()
		{
			if (_requestId == 0)
			{
				_response.SetError(1, "Некорректные параметры вызова");
				return;
			}

			await this.CreateRequest();
		}

		private async Task CreateRequest()
		{
			using (SqlConnection conn = _sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				try
				{
					using (SqlCommand cmd = _sqlServer.GetSpCommand("acquiring.SucceedRequest", conn))
					{
						cmd.AddIntParam("@RequestId", _requestId);
						SqlParameter TypeParam = cmd.AddTinyIntParam("@Type").Output();
						SqlParameter PlaceIdParam = cmd.AddIntParam("@PlaceId").Output();
						SqlParameter EmployeeIdParam = cmd.AddIntParam("@EmployeeId").Output();
						SqlParameter AmountParam = cmd.AddDecimalParam("@Amount", 18, 2).Output();
						SqlParameter retValParam = cmd.AddReturnValue();

						await cmd.ExecuteNonQueryAsync();

						int retVal = retValParam.GetInt32OrDefault();
						if (retVal == -1)
						{
							_response.SetError(3, "Запрос на платёж не найден");
							return;
						}

						_type = (PaymentType)TypeParam.GetByte();
						_placeId = PlaceIdParam.GetInt32();
						_employeeId = EmployeeIdParam.GetInt32OrDefault();
						_amount = AmountParam.GetDecimal();
					}
				}
				catch (Exception e)
				{
					_response.SetError(2, e.Message);
				}
			}
		}
	}
}
