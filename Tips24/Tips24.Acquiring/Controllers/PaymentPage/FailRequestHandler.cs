using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.Acquiring.Controllers.PaymentPage
{
	public class FailRequestHandler
	{
		private Response _response;
		private SqlServer _sqlServer;

		private int _requestId;

		public FailRequestHandler(SqlServer sqlServer)
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
					using (SqlCommand cmd = _sqlServer.GetSpCommand("acquiring.FailRequest", conn))
					{
						cmd.AddIntParam("@RequestId", _requestId);
						SqlParameter RequestIdParam = cmd.AddIntParam("@RequestId").Output();
						await cmd.ExecuteNonQueryAsync();
						int result = RequestIdParam.GetInt32OrDefault();
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
