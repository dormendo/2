using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Tips24.Client.Model;
using Tips24.Client.Services;
using Xamarin.Forms;

namespace Tips24.Client.ViewModels
{
    public class InputTipsAmountViewModel : BaseViewModel
    {
        private int _amount;

        private readonly TipsReceivingOperationContext _tipsReceivingOperationContext;

        public int Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged();
            }
        }

        public int MinAmount { get; set; }
        public int MaxAmount { get; set; }

        public ICommand ContinueWithoutAmountCommand { get; set; }

        public ICommand ContinueCommand { get; set; }

        public InputTipsAmountViewModel(TipsReceivingOperationContext tipsReceivingOperationContext)
        {
            Amount = 200;
            MinAmount = 50;
            MaxAmount = 2000;
            ContinueWithoutAmountCommand = new Command(ContinueWithoutAmount);
            ContinueCommand = new Command(ContinueWithAmount);

            _tipsReceivingOperationContext = tipsReceivingOperationContext;
        }

        private async void ContinueWithoutAmount(object obj)
        {
            _tipsReceivingOperationContext.Amount = 0;
            await Continue();
        }

        private async void ContinueWithAmount(object obj)
        {
            _tipsReceivingOperationContext.Amount = Amount;
            await Continue();
        }

        private async Task Continue()
        {
            switch (_tipsReceivingOperationContext.Mode)
            {
                case TipsReceivingMode.QR:
                    await ServiceLocator.NavigationService.NavigateTo(this, new QRCodeViewModel(_tipsReceivingOperationContext));
                    break;
                case TipsReceivingMode.SMS:
                    await ServiceLocator.NavigationService.NavigateTo(this, new InputPnoneNumberSmsBankViewModel(_tipsReceivingOperationContext));
                    break;
                default:
                    throw new InvalidOperationException($"Не корректный тип операции: {_tipsReceivingOperationContext.Mode}");
            }
        }
    }
}
