using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Tips24.Client.Model;
using Tips24.Client.Services;
using Xamarin.Forms;

namespace Tips24.Client.ViewModels
{
    public class CreatePinCodeRegistrationViewModel : BaseViewModel
    {
        private readonly WaiterRegistrationContext _context;
        private string _pinCode = string.Empty;

        public CreatePinCodeRegistrationViewModel(WaiterRegistrationContext context)
        {
            _context = context;
            PinCodeSize = 4;

            ReturnCommand = new Command(
                async parameter =>
                {
                    _context.PinCode = PinCode;
                    await ServiceLocator.NavigationService.NavigateTo(this, new ConfirmPinCodeRegistrationViewModel(_context));
                });
        }

        public string PinCode
        {
            get => _pinCode; 
            set
            {
                _pinCode = value;
                OnPropertyChanged(nameof(PinCodeCurrentLength));
            }
        }

        public int PinCodeSize { get; private set; }

        public int PinCodeCurrentLength
        {
            get => PinCode.Length;
        }

        public ICommand ReturnCommand { get; private set; }
    }
}
