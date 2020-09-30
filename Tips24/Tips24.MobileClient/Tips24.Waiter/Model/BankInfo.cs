using System;
namespace Tips24.Client.Model
{
    public class BankInfo
    {
        public BankInfo(string id, string onlineServiceName, string smsServiceName)
        {
            Id = id;
            OnlineServiceName = onlineServiceName;
            SmsServiceName = smsServiceName;
        }

        public string Id { get; }

        public string OnlineServiceName { get; }

        public string SmsServiceName { get; }
    }
}
