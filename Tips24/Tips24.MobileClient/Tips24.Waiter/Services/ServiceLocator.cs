using System;
using Tips24.Client.Services.Api;
using Tips24.Client.Services.Impl;

namespace Tips24.Client.Services
{
    public static class ServiceLocator
    {
        static ServiceLocator()
        {
            Vibration = new VibrationService();
            ApplicationContext = new ApplicationContext();
            NavigationService = new NavigationService();
            AuthorizationService = new AuthorizationServiceStub();
            EmployeeInfoStore = new EmployeeInfoStoreStub();
            BankInfoStore = new BankInfoStore();
            ApplicationInfoStore = new ApplicationInfoStore();
            TipsClient = new TipsClientStub();
            //Api = new ApiClient("http://35.233.209.216:8081/", "test 0.0", "dev 0.0");
            Api = new ApiClient("http://localhost:5000/", "test 0.0", "dev 0.0");
		}

        //public static ServiceLocator Instance { get; } = new ServiceLocator();

        public static IAuthorizationService AuthorizationService { get; private set; }

        public static IVibrationService Vibration { get; private set; }

        public static NavigationService NavigationService { get; private set; }

        public static ApplicationContext ApplicationContext { get; private set; }

        public static IEmployeeInfoStore EmployeeInfoStore { get; private set; }

        public static IBankInfoStore BankInfoStore { get; private set; }

        public static IApplicationInfoStore ApplicationInfoStore { get; private set; }

        public static ITipsClient TipsClient { get; private set; }

		public static ApiClient Api { get; private set; }
	}
}
