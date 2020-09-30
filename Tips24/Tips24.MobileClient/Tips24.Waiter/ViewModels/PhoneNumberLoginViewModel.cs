using System;
using System.Windows.Input;
using Tips24.Client.Services;
using Xamarin.Forms;

namespace Tips24.Client.ViewModels
{
    public class PhoneNumberLoginViewModel : BaseViewModel
    {
        private string _phoneNumber;

        public PhoneNumberLoginViewModel()
        {
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                RaiseCanExecuteChanged();
            }
        }

        public ICommand ContinueCommand { get; }

        private bool CanContinue()
        {
            return !string.IsNullOrWhiteSpace(PhoneNumber);
        }

        private async void Continue()
        {
            if (!string.IsNullOrWhiteSpace(_phoneNumber))
            {
                await ServiceLocator.NavigationService.NavigateTo(this, new LoginViewModel(_phoneNumber));
            }
        }

        private void RaiseCanExecuteChanged()
        {
            ((Command)ContinueCommand).ChangeCanExecute();
        }
    }
}
