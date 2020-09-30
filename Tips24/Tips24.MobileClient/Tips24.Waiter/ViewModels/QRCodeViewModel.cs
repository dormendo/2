using System;
using System.Windows.Input;
using Tips24.Client.Model;
using Tips24.Client.Services;
using Xamarin.Forms;

namespace Tips24.Client.ViewModels
{
    public class QRCodeViewModel : BaseViewModel
    {
        private readonly TipsReceivingOperationContext _tipsReceivingOperationContext;

        public QRCodeViewModel(TipsReceivingOperationContext tipsReceivingOperationContext)
        {
            _tipsReceivingOperationContext = tipsReceivingOperationContext;
            TipsUrl = ServiceLocator.ApplicationInfoStore.GetTipsUrl();
            CancelCommand = new Command(Cancel);
        }

        public string PlaceName 
        { 
            get
            {
                return _tipsReceivingOperationContext.PlaceName;
            } 
        }

        public string TipsUrl { get; }

        public ICommand CancelCommand { get; }

        private async void Cancel(object obj)
        {
            await ServiceLocator.NavigationService.NavigateBack<OperationsViewModel>(this);
        }
    }
}
