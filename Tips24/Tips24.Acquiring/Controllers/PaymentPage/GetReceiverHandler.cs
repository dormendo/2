using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.Acquiring.Controllers.PaymentPage
{
	public class GetReceiverHandler
	{
		#region Данные

		private class PlaceData
		{
			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("name")]
			public string Name { get; set; }

			[JsonProperty("city")]
			public string City { get; set; }

			[JsonProperty("address")]
			public string Address { get; set; }
		}

		private class EmployeeData
		{
			[JsonProperty("id")]
			public int Id { get; set; }

			[JsonProperty("first_name")]
			public string FirstName { get; set; }

			[JsonProperty("last_name")]
			public string LastName { get; set; }
		}

		private class GetReceiverResponse : Response
		{
			[JsonProperty("place")]
			public PlaceData Place { get; set; }

			[JsonProperty("employee")]
			public EmployeeData Employee { get; set; }
		}

		#endregion

		private string _receiver;
		private SqlServer _sqlServer;
		private GetReceiverResponse _response;
		private int _placeId;
		private int? _employeeId;

		public GetReceiverHandler(SqlServer sqlServer)
		{
			_sqlServer = sqlServer;
		}

		public async Task<IActionResult> Handle(string receiver)
		{
			_receiver = receiver;
			_response = new GetReceiverResponse();

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

			await this.AcquireData();
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

		private async Task AcquireData()
		{
			using (SqlConnection conn = _sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				try
				{
					using (SqlCommand cmd = _sqlServer.GetSpCommand("acquiring.GetReceiverData", conn))
					{
						cmd.AddIntParam("@PlaceId", _placeId);
						cmd.AddIntParam("@EmployeeId", _employeeId);
						SqlParameter retValParam = cmd.AddReturnValue();

						using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
						{
							if (dr.Read())
							{
								_response.Place = new PlaceData();
								_response.Place.Id = _placeId;
								_response.Place.Name = dr.GetString("DisplayName");
								_response.Place.Address = dr.GetString("Address");
								_response.Place.City = dr.GetString("City");
							}

							if (_employeeId.HasValue)
							{
								dr.NextResult();

								if (dr.Read())
								{
									_response.Employee = new EmployeeData();
									_response.Employee.Id = _employeeId.Value;
									_response.Employee.FirstName = dr.GetString("FirstName");
									_response.Employee.LastName = dr.GetString("LastName");
								}
							}
						}

						int retVal = retValParam.GetInt32OrDefault();

						switch (retVal)
						{
							case -1:
								_response.SetError(-1, "Заведение не найдено");
								break;
							case -2:
								_response.SetError(-2, "Заведение не активно");
								break;
							case -3:
								_response.SetError(-3, "Сотрудник не найден");
								break;
							case -4:
								_response.SetError(-4, "Сотрудник не работает в заведении");
								break;
							case -5:
								_response.SetError(-5, "Нет персональных данных сотрудника");
								break;
						}
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
