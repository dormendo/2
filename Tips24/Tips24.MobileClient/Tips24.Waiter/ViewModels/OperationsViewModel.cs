using System;
using System.Collections.Generic;
using System.Windows.Input;
using Tips24.Client.Model;
using Tips24.Client.Services;
using Xamarin.Forms;

namespace Tips24.Client.ViewModels
{
    public class OperationsViewModel : BaseViewModel
    {
        public ICommand OpenSettingsCommand { get; private set; }

        public List<BaseViewModel> ChildModels { get; private set; }

        public OperationsViewModel()
        {
            OpenSettingsCommand = new Command(OpenSettings);
            ChildModels = new List<BaseViewModel>
            {
                new SelectTipsModeViewModel(new TipsReceivingOperationContext(ServiceLocator.EmployeeInfoStore.PlaceName)),
                new BalanceViewModel(),
                new HelpViewModel(),
                new InfoViewModel()
            };
        }

        private async void OpenSettings(object obj)
        {
            await ServiceLocator.NavigationService.NavigateTo(this, new SettingsViewModel());
        }
    }
}