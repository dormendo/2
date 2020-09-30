using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tips24.PayoutService.PayU
{
	public class PayuHandler
	{
		private SqlServer _sqlServer;
		private ILogger _logger;

		public PayuHandler(SqlServer sqlServer, ILogger<PayuHandler> logger)
		{
			this._sqlServer = sqlServer;
			this._logger = logger;
		}

		public async Task<InitiatePayoutResult> InitiatePayout(PayoutRequestData data)
		{
			Employee employeeData = await this.LoadEmployeeData(data);
			if (employeeData == null)
			{
				return null;
			}

			PayoutRequest request = await this.CreateRequest(employeeData);

			PayuPayoutResult payuResult = await this.SendPayoutRequest(request);
			return null;
		}

		private async Task<Employee> LoadEmployeeData(PayoutRequestData data)
		{
			return null;
		}

		private async Task<PayoutRequest> CreateRequest(Employee employeeData)
		{
			return null;
		}

		private async Task<PayuPayoutResult> SendPayoutRequest(PayoutRequest request)
		{
			return null;
		}
	}
}
