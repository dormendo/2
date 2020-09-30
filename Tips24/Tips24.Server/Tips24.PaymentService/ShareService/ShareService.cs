using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Server;

namespace Tips24.PaymentService.Share
{
	public class ShareService
	{
		private static SqlMetaData[] _amountsMetadata;

		private SqlServer _sqlServer;
		private ILogger _logger;

		static ShareService()
		{
			_amountsMetadata = new SqlMetaData[]
			{
				new SqlMetaData("EmployeeId", SqlDbType.Int),
				new SqlMetaData("Amount", SqlDbType.Decimal, 18, 2)
			};
		}

		public ShareService(SqlServer sqlServer, ILogger<ShareService> logger)
		{
			this._sqlServer = sqlServer;
			this._logger = logger;
		}

		public async Task<bool> ProceedPayment(Payment payment, SqlConnection conn, SqlTransaction tx)
		{
			if (await this.CheckForExistingPayment(payment, conn, tx))
			{
				ShareData share = await this.GetShareData(payment, conn, tx);
				this.CalculateAmounts(payment, share);
				await this.SavePayment(payment, conn, tx);
				return true;
			}

			return false;
		}

		internal async Task SaveDocumentProperties(Document doc)
		{
			using (SqlConnection conn = _sqlServer.GetConnection())
			{
				await conn.OpenAsync();

				using (SqlCommand cmd = _sqlServer.GetSpCommand("payment.SaveDocumentProperties", conn))
				{
					cmd.AddIntParam("@DocumentId", doc.DocumentId);
					cmd.AddVarCharParam("@DocumentNumber", 40, doc.DocumentNumber);
					cmd.AddDateParam("@DocumentDate", doc.DocumentDate);

					await cmd.ExecuteNonQueryAsync();
				}
			}
		}


		#region Проверка существования такого платежа

		private async Task<bool> CheckForExistingPayment(Payment payment, SqlConnection conn, SqlTransaction tx)
		{
			using (SqlCommand cmd = _sqlServer.GetSpCommand("payment.CheckForExistingPayment", conn, tx))
			{
				cmd.AddCharParam("@DataSource", 6, payment.DataSource);
				cmd.AddVarCharParam("@DocumentName", 100, payment.DocumentName);
				cmd.AddVarCharParam("@ExternalId", 50, payment.ExternalId);

				SqlParameter retValParam = cmd.AddReturnValue();

				await cmd.ExecuteNonQueryAsync();

				int retVal = retValParam.GetInt32OrDefault();
				return retVal == 0;
			}
		}

		#endregion

		#region Загрузка данных о распределении

		private async Task<ShareData> GetShareData(Payment payment, SqlConnection conn, SqlTransaction tx)
		{
			ShareData share = new ShareData();
			Dictionary<int, GroupShareData> groups = new Dictionary<int, GroupShareData>();

			using (SqlCommand cmd = _sqlServer.GetSpCommand("payment.LoadShareData", conn, tx))
			{
				cmd.AddIntParam("@PlaceId", payment.PlaceId);
				cmd.AddIntParam("@EmployeeId", payment.EmployeeId);
				cmd.AddDateTime2Param("@PaymentDateTime", payment.PaymentDateTime);
				cmd.AddBitParam("@IsTimeSpecified", payment.IsTimeSpecified);

				SqlParameter PaymentLimitParam = cmd.AddDecimalParam("@PaymentLimit", 18, 2).Output();
				SqlParameter SystemCommissionParam = cmd.AddDecimalParam("@SystemCommission", 4, 2).Output();
				SqlParameter IsPlaceActiveParam = cmd.AddIntParam("@IsPlaceActive").Output();
				SqlParameter PlaceDisplayNameParam = cmd.AddNVarCharParam("@PlaceDisplayName", 100).Output();
				SqlParameter ShareSchemeHistoryIdParam = cmd.AddIntParam("@ShareSchemeHistoryId").Output();
				SqlParameter PersonalShareParam = cmd.AddTinyIntParam("@PersonalShare").Output();
				SqlParameter EmployeeFirstNameParam = cmd.AddNVarCharParam("@EmployeeFirstName", 50).Output();
				SqlParameter EmployeeLastNameParam = cmd.AddNVarCharParam("@EmployeeLastName", 50).Output();
				SqlParameter EmployeeIsFiredParam = cmd.AddBitParam("@EmployeeIsFired").Output();

				using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
				{
					while (dr.Read())
					{
						GroupShareData gdata = new GroupShareData();
						gdata.Name = dr.GetString("Name");
						gdata.Id = dr.GetInt32("GroupId");
						gdata.Weight = dr.GetByte("GroupWeight");
						groups.Add(gdata.Id, gdata);
						share.Groups.Add(gdata);
					}

					dr.NextResult();

					while (dr.Read())
					{
						MembershipData md = new MembershipData();
						md.EmployeeId = dr.GetInt32("EmployeeId");
						md.GroupId = dr.GetInt32("GroupId");
						md.BeginDateTime = dr.GetDateTime("BeginDateTime");
						md.EndDateTime = dr.GetDateTime("EndDateTime");
						md.IsManager = dr.GetBoolean("IsManager");
						md.IsOwner = dr.GetBoolean("IsOwner");
						share.Memberships.Add(md);

						groups[md.GroupId].AddMembership(md);
					}
				}

				share.PaymentLimit = PaymentLimitParam.GetDecimal();
				share.SystemCommission = SystemCommissionParam.GetDecimal();
				share.Place.Id = payment.PlaceId;
				share.Place.Name = PlaceDisplayNameParam.Value.ToString();
				share.Place.IsActive = (IsPlaceActiveParam.GetInt32() != 0);
				share.ShareSchemeHistoryId = ShareSchemeHistoryIdParam.GetInt32();
				share.PersonalShare = PersonalShareParam.GetByte();

				if (payment.EmployeeId.HasValue)
				{
					share.Receiver = new ReceiverData();
					share.Receiver.Id = payment.EmployeeId.Value;
					share.Receiver.FirstName = EmployeeFirstNameParam.Value.ToString();
					share.Receiver.LastName = EmployeeLastNameParam.Value.ToString();
					share.Receiver.IsFired = EmployeeIsFiredParam.GetBooleanOrDefault();
				}
			}

			return share;
		}

		#endregion

		#region Расчёт распределения средств

		private void CalculateAmounts(Payment payment, ShareData share)
		{
			if (payment.OriginalAmount > share.PaymentLimit)
			{
				payment.Status = PaymentStatus.ToReturn;
				payment.ReasonToReturn = ReasonToReturnType.AmountLimitExceeded;
				return;
			}

			if (!share.Place.IsActive)
			{
				payment.Status = PaymentStatus.ToReturn;
				payment.ReasonToReturn = ReasonToReturnType.PlaceInactive;
				return;
			}

			decimal systemAmount = decimal.Floor(payment.OriginalAmount * share.SystemCommission) / 100M;
			decimal employeesAmount = payment.OriginalAmount - systemAmount;

			AmountData amountData = this.CalculateEmployeeAmounts(employeesAmount, share, payment);
			payment.PayoutAmount = amountData.PayoutAmount;

			payment.ShareSchemeHistoryId = share.ShareSchemeHistoryId;

			foreach (PersonalAmountData pad in amountData.PersonalAmounts.Values)
			{
				if (pad.Amount > 0M)
				{
					payment.PersonalAmounts.Add(new PersonalAmount { EmployeeId = pad.EmployeeId, Amount = pad.Amount });
				}
			}
		}

		private AmountData CalculateEmployeeAmounts(decimal amount, ShareData share, Payment payment)
		{
			AmountData amountData = new AmountData();
			decimal personalAmount = 0M;
			if (share.Receiver != null && !share.Receiver.IsFired)
			{
				personalAmount = decimal.Floor(amount * share.PersonalShare) / 100M;
				amountData.AddPersonalAmount(share.Receiver.Id, personalAmount);
			}

			decimal groupAmount = amount - personalAmount;

			if (groupAmount == 0M)
			{
				return amountData;
			}

			decimal totalWeight = 0M;
			foreach (GroupShareData groupShare in share.Groups)
			{
				groupShare.CalculateGroupWeight(payment.IsTimeSpecified);
				totalWeight += groupShare.GroupWeight;
			}

			if (totalWeight == 0M)
			{
				if (share.Receiver == null)
				{
					foreach (GroupShareData groupShare in share.Groups)
					{
						groupShare.Weight = 1M;
						groupShare.CalculateGroupWeight(payment.IsTimeSpecified);
						totalWeight += groupShare.GroupWeight;
					}
				}

				if (totalWeight == 0M)
				{
					if (share.Receiver == null || share.Receiver.IsFired)
					{
						payment.Status = PaymentStatus.ToReturn;
						payment.ReasonToReturn = ReasonToReturnType.NoWeightsToShare;
					}
					else
					{
						amountData.AddPersonalAmount(share.Receiver.Id, groupAmount);
					}

					return amountData;
				}
			}

			if (payment.IsTimeSpecified)
			{
				foreach (GroupShareData groupShare in share.Groups)
				{
					decimal planGroupAmount = groupAmount * groupShare.GroupWeight / totalWeight;
					if (planGroupAmount == 0M)
					{
						continue;
					}

					decimal count = (decimal)groupShare.Memberships.GetMemberCount();
					if (count == 0M)
					{
						continue;
					}

					foreach (int employeeId in groupShare.Memberships.EnumerateMembers())
					{
						amountData.AddPersonalAmount(employeeId, planGroupAmount / count);
					}
				}
			}
			else
			{
				foreach (GroupShareData groupShare in share.Groups)
				{
					decimal planGroupAmount = groupAmount * groupShare.GroupWeight / totalWeight;
					if (planGroupAmount == 0M)
					{
						continue;
					}

					foreach (MembershipData md in groupShare.Memberships.EnumeratePartsOfDay())
					{
						if (md.PartOfDay == 0M || groupShare.GroupTotalDays == 0M)
						{
							continue;
						}

						amountData.AddPersonalAmount(md.EmployeeId, planGroupAmount * md.PartOfDay / groupShare.GroupTotalDays);
					}
				}
			}

			if (amountData.PersonalAmounts.Count == 1)
			{
				amountData.PersonalAmounts.First().Value.Amount = amount;
			}
			else
			{
				foreach (PersonalAmountData pad in amountData.PersonalAmounts.Values)
				{
					pad.NormalizeAmount();
					amountData.PayoutAmount += pad.Amount;
				}
			}

			return amountData;
		}

		#endregion

		#region Сохранение платежа

		private async Task SavePayment(Payment payment, SqlConnection conn, SqlTransaction tx)
		{
			DataAccess.StructuredParamValue amounts = null;
			if (payment.PersonalAmounts.Count > 0)
			{
				amounts = new DataAccess.StructuredParamValue(_amountsMetadata, payment.PersonalAmounts.Count);
				foreach (PersonalAmount pa in payment.PersonalAmounts)
				{
					amounts.NewRecord();
					amounts.AddInt32(pa.EmployeeId);
					amounts.AddDecimal(pa.Amount);
				}
			}

			using (SqlCommand cmd = _sqlServer.GetSpCommand("payment.SavePayment", conn, tx))
			{
				cmd.AddIntParam("@PlaceId", payment.PlaceId);
				cmd.AddIntParam("@EmployeeId", payment.EmployeeId);
				cmd.AddIntParam("@ShareSchemeHistoryId", payment.ShareSchemeHistoryId);
				cmd.AddTinyIntParam("@Status", (byte)payment.Status);
				cmd.AddTinyIntParam("@ReasonToReturn", (byte?)payment.ReasonToReturn);
				cmd.AddCharParam("@DataSource", 6, payment.DataSource);
				cmd.AddCharParam("@Provider", 6, payment.Provider);
				cmd.AddDecimalParam("@OriginalAmount", 18, 2, payment.OriginalAmount);
				cmd.AddDecimalParam("@ReceivedAmount", 18, 2, payment.ReceivedAmount);
				cmd.AddDecimalParam("@BankCommissionAmount", 18, 2, payment.BankCommissionAmount);
				cmd.AddDecimalParam("@AgentCommissionAmount", 18, 2, payment.AgentCommissionAmount);
				cmd.AddDecimalParam("@IncomeAmount", 18, 2, payment.IncomeAmount);
				cmd.AddDecimalParam("@PayoutAmount", 18, 2, payment.PayoutAmount);
				cmd.AddDateTime2Param("@PaymentDateTime", payment.PaymentDateTime);
				cmd.AddBitParam("@IsTimeSpecified", payment.IsTimeSpecified);
				cmd.AddDateTime2Param("@ArrivalDateTime", payment.ArrivalDateTime);
				cmd.AddVarCharParam("@DocumentName", 100, (payment.DocumentId.HasValue ? null : payment.DocumentName));
				SqlParameter documentIdParam = cmd.AddIntParam("@DocumentId", payment.DocumentId).InputOutput();
				cmd.AddVarCharParam("@DocumentNumber", 40, (payment.DocumentId.HasValue ? null : payment.DocumentNumber));
				cmd.AddDateParam("@DocumentDate", (payment.DocumentId.HasValue ? null : payment.DocumentDate));
				cmd.AddVarCharParam("@ExternalId", 50, payment.ExternalId);
				cmd.AddNVarCharParam("@Fio", 100, payment.Fio);
				cmd.AddNVarCharParam("@Address", 150, payment.Address);
				cmd.AddNVarCharParam("@Purpose", 150, payment.Purpose);
				cmd.AddNVarCharMaxParam("@RawData", payment.RawData);
				cmd.AddStructuredParam("@Amounts", "payment.PaymentShare", amounts);
				SqlParameter PaymentIdParam = cmd.AddBigIntParam("@PaymentId").Output();
				SqlParameter StatusParam = cmd.AddTinyIntParam("@FinalStatus").Output();

				await cmd.ExecuteNonQueryAsync();

				payment.Id = PaymentIdParam.GetInt64();
				payment.Status = (PaymentStatus)StatusParam.GetByte();
				if (!payment.DocumentId.HasValue)
				{
					payment.DocumentId = documentIdParam.GetInt32OrNull();
				}
			}
		}

		#endregion
	}
}
