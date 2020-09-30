using System;
using System.Windows.Input;
using Tips24.Client.Model;
using Tips24.Client.Services;
using Xamarin.Forms;

namespace Tips24.Client.ViewModels
{
    public class PhoneNumberRegistrationViewModel : BaseViewModel
    {
        private readonly WaiterRegistrationContext _context;
        private string _phoneNumber;

        public PhoneNumberRegistrationViewModel(WaiterRegistrationContext context)
        {
            ContinueCommand = new Command(Continue, CanContinue);
            _context = context;
        }

        public string PlaceName 
        { 
            get { return _context.PlaceName; } 
        }

        public string PlaceAddress
        {
            get { return _context.PlaceAddress; }
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
                _context.PhoneNumber = _phoneNumber;
                await ServiceLocator.NavigationService.NavigateTo(this, new ConfirmPhoneNumberViewModel(_context));
            }
         }

        private void RaiseCanExecuteChanged()
        {
            ((Command)ContinueCommand).ChangeCanExecute();
        }
    }
}
