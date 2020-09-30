using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.Acquiring.Controllers.PaymentPage
{
	public class SaveOrderIdHandler
	{
		private Response _response;
		private SqlServer _sqlServer;

		private int _requestId;
		private string _orderId;

		public SaveOrderIdHandler(SqlServer sqlServer)
		{
			_sqlServer = sqlServer;
		}

		public async Task<IActionResult> Handle(int requestId, string orderId)
		{
			_requestId = requestId;
			_orderId = orderId;
			_response = new Response();

			await this.HandleInternal();

			if (_response.HasError)
			{
				Helper.SaveDiagMessage(_sqlServer, DiagOptions.Tech, _response.ErrorMessage, null);
			}

			return new JsonResult(_response);
		}

		private async Task HandleInternal()
		{
			if (_requestId == 0 || string.IsNullOrEmpty(_orderId))
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
					using (SqlCommand cmd = _sqlServer.GetSpCommand("acquiring.SaveOrderId", conn))
					{
						cmd.AddIntParam("@RequestId", _requestId);
						cmd.AddVarCharParam("@OrderId", 50, _orderId);

						await cmd.ExecuteNonQueryAsync();
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
