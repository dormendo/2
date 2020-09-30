using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tips24.Client.Services;
using Tips24.Client.ViewModels;
using Xamarin.Forms;

namespace Tips24.Client
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

		public MainPage(MainPageViewModel viewModel) : this()
		{
			this.BindingContext = viewModel;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			Task.Run(() => LoadData());
		}

		private void LoadData()
		{
			string launchingLink = ServiceLocator.NavigationService.LaunchingLink;
			if (launchingLink != null)
			{

			}
			else
			{

			}
		}
	}
}
