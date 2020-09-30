using System;
using System.Windows.Input;
using Tips24.Client.Model;
using Tips24.Client.Services;
using Xamarin.Forms;

namespace Tips24.Client.ViewModels
{
    public class RegistrationViewModel : BaseViewModel
    {
        private readonly WaiterRegistrationContext _context;

        public string Surname
        {
            get => _context.Surname; 
            set
            {
                _context.Surname = value;
                RaiseCanExecuteChanged();
            }
        }

        public string Name
        {
            get => _context.Name; 
            set
            {
                _context.Name = value;
                RaiseCanExecuteChanged();
            }
        }

        public ICommand ContinueCommand { get; }

        public RegistrationViewModel(WaiterRegistrationContext context)
        {
            _context = context;
            ContinueCommand = new Command(Continue, CanContinue);
        }

        private bool CanContinue()
        {
            return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Surname);
        }

        private async void Continue()
        {
            await ServiceLocator.NavigationService.NavigateTo(this, new CreatePinCodeRegistrationViewModel(_context));
        }

        private void RaiseCanExecuteChanged()
        {
            ((Command)ContinueCommand).ChangeCanExecute();
        }
    }
}
