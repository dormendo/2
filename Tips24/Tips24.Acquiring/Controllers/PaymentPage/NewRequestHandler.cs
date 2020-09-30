using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.Acquiring.Controllers.PaymentPage
{
	public class NewRequestHandler
	{
		#region Данные

		public class Response : PaymentPage.Response
		{
			[JsonProperty("request_id")]
			public int RequestId { get; set; }
		}

		#endregion


		private string _receiver;
		private Response _response;
		private SqlServer _sqlServer;
		private int _placeId;
		private int? _employeeId;

		private int _sum;
		private string _ip;
		private string _method;
		private PaymentType _type;

		public NewRequestHandler(SqlServer sqlServer)
		{
			_sqlServer = sqlServer;
		}

		public async Task<IActionResult> Handle(string receiver, int sum, string ip, PaymentType type, string method = null)
		{
			_receiver = receiver;
			_sum = sum;
			_ip = ip;
			_method = method;
			_type = type;
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
			try
			{
				this.ParseReceiver();
			}
			catch
			{
				_response.SetError(1, "Код получателя имеет некорректный формат");
				return;
			}

			await this.CreateRequest();
		}

		private void ParseReceiver()
		{
			int dashIndex = _receiver.IndexOf('-');

			string placeStr = null, employeeStr = null;
			if (dashIndex < 0)
			{
				placeStr = _receiver;
			}
			else
			{
				placeStr = _receiver.Substring(0, dashIndex);
				employeeStr = _receiver.Substring(dashIndex + 1, _receiver.Length - dashIndex - 1);
			}

			_placeId = int.Parse(placeStr);
			if (employeeStr != null)
			{
				_employeeId = int.Parse(employeeStr);
			}
		}

		private async Task CreateRequest()
		{
			using (SqlConnection conn = _sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				try
				{
					using (SqlCommand cmd = _sqlServer.GetSpCommand("acquiring.CreateRequest", conn))
					{
						cmd.AddTinyIntParam("@Type", (byte)_type);
						cmd.AddIntParam("@PlaceId", _placeId);
						cmd.AddIntParam("@EmployeeId", _employeeId);
						cmd.AddDecimalParam("@Amount", 18, 2, _sum);
						cmd.AddVarCharParam("@IpAddress", 15, _ip);
						cmd.AddNVarCharParam("@PaymentMethod", 100, _method);

						SqlParameter RequestIdParam = cmd.AddIntParam("@RequestId").Output();

						await cmd.ExecuteNonQueryAsync();

						_response.RequestId = RequestIdParam.GetInt32OrDefault();
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
