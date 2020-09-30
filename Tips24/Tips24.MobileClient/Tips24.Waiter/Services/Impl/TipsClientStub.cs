using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Tips24.Client.Services.Impl
{
    public class TipsClientStub : ITipsClient
    {
        public Task StartSmsPayment(string phone, decimal amount, string bankCode)
        {
            Debug.WriteLine($"Отправлено смс на номер {phone} с суммой {amount}");
            return Task.CompletedTask;
        }
    }
}
