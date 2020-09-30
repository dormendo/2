using System;
using Tips24.Client.Model;

namespace Tips24.Client.Services.Impl
{
    public class BankInfoStore : IBankInfoStore
    {
        public BankInfo Get(string code)
        {
            if (code == "alfa")
            {
                return new BankInfo("alfa", "Альфа-Клик", "Alfabank");
            }

            if (code == "sber")
            {
                return new BankInfo("sber", "Сбербанк-Онлайн", "900 (Сбербанк)");
            }

            throw new ArgumentException($"{code}");
        }
    }
}
