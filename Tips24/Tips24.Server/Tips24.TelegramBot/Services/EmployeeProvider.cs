using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.TelegramBot
{
	public class EmployeeProvider
	{
		private SqlServer _sqlServer;

		public EmployeeProvider(SqlServer sqlServer)
		{
			this._sqlServer = sqlServer;
		}

		public async Task<Employee> GetEmployeeByTelegramUserIdAsync(long telegramUserId)
		{
			using (SqlConnection conn = this._sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				using (SqlCommand cmd = this._sqlServer.GetSpCommand("telegram.GetUserRecordByUserId", conn))
				{
					cmd.AddBigIntParam("@UserId", telegramUserId);
					using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
					{
						if (dr.Read())
						{
							return AcquireEmployee(telegramUserId, dr);
						}

						return null;
					}
				}
			}
		}

		public async Task<Employee> SetPhoneAndGetEmployee(long telegramUserId, string phone)
		{
			if (string.IsNullOrEmpty(phone) || (phone.Length == 12 && !phone.StartsWith("+7")) || (phone.Length == 11 && !phone.StartsWith("7")) || phone.Length < 11 || phone.Length > 12)
			{
				return null;
			}

			if (phone.Length == 12)
			{
				phone = phone.Substring(2, 10);
			}
			else
			{
				phone = phone.Substring(1, 10);
			}

			using (SqlConnection conn = _sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				using (SqlCommand cmd = _sqlServer.GetSpCommand("telegram.CreateAndGetUserRecord", conn))
				{
					cmd.AddBigIntParam("@UserId", telegramUserId);
					cmd.AddCharParam("@Phone", 10, phone);

					using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
					{
						if (dr.Read())
						{
							return AcquireEmployee(telegramUserId, dr);
						}

						return null;
					}
				}
			}
		}

		private static Employee AcquireEmployee(long telegramUserId, SqlDataReader dr)
		{
			Employee rec = new Employee();
			rec.TelegramUserId = telegramUserId;
			rec.Id = dr.GetInt32("EmployeeId");
			rec.Phone = dr.GetString("Phone");
			rec.FirstName = dr.GetString("FirstName");
			rec.LastName = dr.GetString("LastName");
			rec.Balance = dr.GetDecimal("Balance");
			rec.IsFired = dr.GetBoolean("IsFired");
			rec.IsManager = dr.GetBoolean("IsManager");
			rec.IsOwner = dr.GetBoolean("IsOwner");

			rec.Place = new PlaceData();
			rec.Place.Id = dr.GetInt32("PlaceId");
			rec.Place.Name = dr.GetString("PlaceName");
			rec.Place.Address = dr.GetString("PlaceAddress");
			rec.Place.City = dr.GetString("PlaceCity");
			rec.Place.IsSchemeIndividual = dr.GetBoolean("IsPlaceSchemeIndividual");
			rec.Place.IsActive = (dr.GetInt32("IsPlaceActive") != 0);

			rec.Group = new GroupData();
			rec.Group.Id = dr.GetInt32("GroupId");
			rec.Group.Name = dr.GetString("GroupName");

			string qrCodeTelegramFileId = dr.GetStringOrNull("QrCodeFileId");
			if (qrCodeTelegramFileId != null)
			{
				rec.QrCode = new QrCodeData();
				rec.QrCode.FileId = qrCodeTelegramFileId;
				rec.QrCode.StringHash = dr.GetBytes("QrCodeStringHash");
				rec.QrCode.CreateDateTime = dr.GetDateTime("QrCodeDateTime");
			}

			int? turnId = dr.GetInt32OrNull("TurnSeqNum");
			if (turnId.HasValue)
			{
				rec.Turn = new TurnData();
				rec.Turn.TurnId = turnId.Value;
				rec.Turn.BeginDateTime = dr.GetDateTime("TurnBeginDateTime");
				rec.Turn.EndDateTime = dr.GetDateTime("TurnEndDateTime");
			}

			byte? dialogSessionType = dr.GetByteOrNull("DialogSessionType");
			if (dialogSessionType.HasValue && (dialogSessionType.Value == 0 || dialogSessionType.Value == 1))
			{
				if (dialogSessionType.Value == 0)
				{
					rec.DialogSession = new YandexKassaSessionData();
				}
				else
				{
					rec.DialogSession = new TurnSessionData();
				}

				rec.DialogSession.StepByte = dr.GetByte("DialogStep");
				rec.DialogSession.DeserializeFromJson(dr.GetStringOrNull("DialogData"));
			}

			return rec;
		}
	}
}
