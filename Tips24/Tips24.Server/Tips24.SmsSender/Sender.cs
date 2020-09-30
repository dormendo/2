using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;

namespace Tips24.SmsSender
{
	class Sender
	{
		private CancellationTokenSource _cts;

		public Sender(CancellationTokenSource cts)
		{

			_cts = cts;
		}

		internal void Send()
		{
			SqlServer sqlServer = new SqlServer();
			while (!_cts.IsCancellationRequested)
			{
				using (SqlConnection conn = sqlServer.GetConnection())
				{
					conn.Open();
					using (SqlCommand command = sqlServer.GetCommand("SELECT TOP(1) MessageId, MessageType, MessageParams FROM dbo.SmsMessage with(readpast) ORDER BY MessageId", conn))
					{
						using (SqlDataReader dr = command.ExecuteReader())
						{
							if (dr.Read())
							{

							}
						}
					}
				}
			}
		}
	}
}
