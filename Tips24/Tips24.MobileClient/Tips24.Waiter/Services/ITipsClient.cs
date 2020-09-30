using System;
using System.Threading.Tasks;

namespace Tips24.Client.Services
{
    /// <summary>
    /// Взаимодействие с сервером.
    /// </summary>
    public interface ITipsClient
    {
        Task StartSmsPayment(string phone, decimal amount, string bankCode);
    }
}
