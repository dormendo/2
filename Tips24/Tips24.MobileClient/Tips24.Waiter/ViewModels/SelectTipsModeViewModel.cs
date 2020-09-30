using System;
using System.Windows.Input;
using Tips24.Client.Model;
using Tips24.Client.Services;
using Xamarin.Forms;

namespace Tips24.Client.ViewModels
{
    public class SelectTipsModeViewModel : BaseViewModel
    {
        private readonly TipsReceivingOperationContext _tipsReceivingOperationContext;

        public ICommand SelectSberQRModeCommand { get; private set; }

        public ICommand SelectSmsBankModeCommand { get; private set; }

        public SelectTipsModeViewModel(TipsReceivingOperationContext tipsReceivingOperationContext)
        {
            _tipsReceivingOperationContext = tipsReceivingOperationContext;
 
            SelectSberQRModeCommand = new Command(SelectSberQRMode);
            SelectSmsBankModeCommand = new Command(SelectSmsBankMode);
        }

        private async void SelectSberQRMode()
        {
            _tipsReceivingOperationContext.Mode = TipsReceivingMode.QR;
            await ServiceLocator.NavigationService.NavigateTo(this, new InputTipsAmountViewModel(_tipsReceivingOperationContext));
        }

        private async void SelectSmsBankMode()
        {
            _tipsReceivingOperationContext.Mode = TipsReceivingMode.SMS;

            await ServiceLocator.NavigationService.NavigateTo(this, new SelectBankViewModel(_tipsReceivingOperationContext));
        }
    }
}
