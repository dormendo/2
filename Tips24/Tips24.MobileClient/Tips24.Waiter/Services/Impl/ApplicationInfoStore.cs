using System;

namespace Tips24.Client.Services.Impl
{
    public class ApplicationInfoStore : IApplicationInfoStore
    {
        public ApplicationInfoStore()
        {
        }

        public string GetTipsUrl()
        {
            return "www.tips24.ru/3";
        }
    }
}
