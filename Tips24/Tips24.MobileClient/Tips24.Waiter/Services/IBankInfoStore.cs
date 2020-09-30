using System;
using Tips24.Client.Model;

namespace Tips24.Client.Services
{
    public interface IBankInfoStore
    {
        BankInfo Get(string code);
    }
}
