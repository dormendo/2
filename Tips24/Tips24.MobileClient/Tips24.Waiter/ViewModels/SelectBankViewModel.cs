using System;
using System.Windows.Input;
using Tips24.Client.Model;
using Tips24.Client.Services;
using Xamarin.Forms;

namespace Tips24.Client.ViewModels
{
    public class SelectBankViewModel : BaseViewModel
    {
        public TipsReceivingOperationContext _tipsReceivingOperationContext;

        public SelectBankViewModel(TipsReceivingOperationContext tipsReceivingOperationContext)
        {
            _tipsReceivingOperationContext = tipsReceivingOperationContext;

            SelectBankCommand = new Command(o => SelectBank((string)o));
        }

        public ICommand SelectBankCommand { get; }

        private async void SelectBank(string bankCode)
        {
            _tipsReceivingOperationContext.Bank = ServiceLocator.BankInfoStore.Get(bankCode);

            await ServiceLocator.NavigationService.NavigateTo(this, new InputTipsAmountViewModel(_tipsReceivingOperationContext));
        }
    }
}
