using System;
using System.Windows.Input;
using Tips24.Client.Model;
using Tips24.Client.Services;
using Xamarin.Forms;

namespace Tips24.Client.ViewModels
{
    public class ConfirmSmsBankViewModel : BaseViewModel
    {
        private readonly TipsReceivingOperationContext _tipsReceivingOperationContext;

        public ConfirmSmsBankViewModel(TipsReceivingOperationContext tipsReceivingOperationContext)
        {
            _tipsReceivingOperationContext = tipsReceivingOperationContext;
            ConfirmCommand = new Command(Confirm);
        }

        public decimal Amount
        {
            get { return _tipsReceivingOperationContext.Amount; }
        }

        public string PlaceName
        {
            get { return _tipsReceivingOperationContext.PlaceName; }
        }

        public string TipsUrl
        {
            get { return ServiceLocator.ApplicationInfoStore.GetTipsUrl(); }
        }

        public string ConfirmCommandTitle
        {
            get { return $"Подтвердить по СМС от {_tipsReceivingOperationContext.Bank.OnlineServiceName}"; }
        }

        public ICommand ConfirmCommand { get; }

        private async void Confirm()
        {
            await ServiceLocator.TipsClient.StartSmsPayment(
                _tipsReceivingOperationContext.PhoneNumber,
                _tipsReceivingOperationContext.Amount, 
                _tipsReceivingOperationContext.Bank.Id);

            var message = $"Чтобы закончить перевод чаевых, ответьте на СМС от {_tipsReceivingOperationContext.Bank.OnlineServiceName}";
            await ServiceLocator.NavigationService.DisplayMessage(null, message, "Понятно");
            await ServiceLocator.NavigationService.NavigateBack<OperationsViewModel>(this);
        }
    }
}
