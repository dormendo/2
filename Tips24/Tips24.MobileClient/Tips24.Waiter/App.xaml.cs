using System;
using System.Linq;
using System.Threading.Tasks;
using Tips24.Client.Model;
using Tips24.Client.Pages;
using Tips24.Client.Services;
using Tips24.Client.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Tips24.Client
{
    public partial class App : Application
	{
        public const string SyncfusionLicense = "MzY0MzNAMzEzNjJlMzMyZTMwSFNVejBEQS96WVNFUEltSzQ1TEk5Vko2UHlXTVpNamFUaFpQaUVIWDYvQT0=;MzY0MzRAMzEzNjJlMzMyZTMwUkJhc3NzVGZjZ29pSEx0TElxQkhPMW9VTCtKSG1sNk1FT0dHU29JY1NaUT0=;MzY0MzVAMzEzNjJlMzMyZTMwaE5VdU5GUStEc1BwRjU0WGU0VzYycGNJUUVCTldTWTRhK3ZEZ2R0Uy9ucz0=;MzY0MzZAMzEzNjJlMzMyZTMwWXVIY3JZOVZEQ3RqVTdUQzdWazZiYThha2xaYkhUb2l1ZXBRekdRMTdOaz0=;MzY0MzdAMzEzNjJlMzMyZTMwai9ZRDljZkhRNi9GVVB5enc4N3ZOZUhZWjZqeUIvbXdxN3BoVDFxaktGdz0=;MzY0MzhAMzEzNjJlMzMyZTMwT2RmSVpWaHZkN01LYVR3TkYzSlpFVEFMOGJidURjVWZOcThuY2xKMFBvND0=;MzY0MzlAMzEzNjJlMzMyZTMwZDVUTjIwTGdQUTZNanorMWVJcCtKQ1loSWpwVy9DcmlzdEZrMW1NWU9Qaz0=;MzY0NDBAMzEzNjJlMzMyZTMwa1laNGs2bXpkTDA3OVNLZXlXVE9LWnBTekRVZ3JmZjZEQlBRSi8xYkQvYz0=;MzY0NDFAMzEzNjJlMzMyZTMwbHNxei9DSjdta3l4SWV0eUM0eDJTNWEvRHQvUldTbjBIQVFzMlF2b1JZTT0=";

		public App(string launchingLink)
        {
            //Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(SyncfusionLicense);

            this.InitializeComponent();
            ServiceLocator.NavigationService.Initialize(launchingLink);

            if (ServiceLocator.AuthorizationService.GetRegularAuthKey() == null)
            {
                var waiterRegistrationContext = new WaiterRegistrationContext("testPlace", "testAddress", 1, Guid.Parse("00000000-0000-0000-0000-000000000001"));
                MainPage = new NavigationPage(new PhoneNumberRegistrationPage
                {
                    BindingContext = new PhoneNumberRegistrationViewModel(waiterRegistrationContext)
                });
            }
            else
            {
                MainPage = new NavigationPage(new OperationsPage(new OperationsViewModel()));
            }

            //MainPage = new NavigationPage(new MainPage(new ViewModels.MainPageViewModel()));
        }

        public App() : this(null)
        {
        }


        private static Page AcquireStartPage(string lauchingLink)
		{
			if (string.IsNullOrEmpty(lauchingLink))
			{
				if (ServiceLocator.AuthorizationService.GetRegularAuthKey() == null)
				{
					return new PhoneNumberLoginPage(new PhoneNumberLoginViewModel());
				}
				else
				{
					return new MainPage(new MainPageViewModel());
				}
			}
			else
			{
				return new MainPage(new MainPageViewModel());
			}
		}

        protected override void OnStart()
        {
			ServiceLocator.Api.Start();
		}

		protected override void OnSleep()
        {
			ServiceLocator.Api.Stop();
		}

		protected override void OnResume()
        {
			ServiceLocator.Api.Start();
		}
	}
}
