using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Tips24.Client.Services;
using Xamarin.Forms;

namespace Tips24.Client.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _pinCode = string.Empty;
        private readonly string _phoneNumber;

        public int PinCodeSize { get; private set; }

        public int PinCodeCurrentLength 
        { 
            get { return _pinCode.Length; }
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

        public ICommand ReturnCommand { get; private set; }

        public ICommand ChangePinCodeCommand { get; private set; }

        public LoginViewModel(string phoneNumber)
        {
            PinCodeSize = 4;
            _phoneNumber = phoneNumber;
            ReturnCommand = new Command(Continue);
            ChangePinCodeCommand = new Command(ChangePinCode);
        }

        private async void ChangePinCode(object obj)
        {
            await ServiceLocator.NavigationService.DisplayMessage(null,"не реализоввано");
        }

        private async void Continue()
        {
            if (!string.IsNullOrWhiteSpace(PinCode))
            {
                var responce = await ServiceLocator.Api.Auth.Login(new Dto.Auth.LoginRequest
                {
                    Phone = _phoneNumber,
                    PinCode = _pinCode
                });

                switch (responce.Kind)
                {
                    case Services.Api.ApiResponseKind.Success:
                        ServiceLocator.AuthorizationService.SetRegularAuthKey(responce.Response.PermanentKey);
                        await ServiceLocator.NavigationService.NavigateTo(this, new OperationsViewModel());
                        break;
                    case Services.Api.ApiResponseKind.Error:
                        _pinCode = string.Empty;
                        //todo обработать неправильный пин/телефон
                        await ServiceLocator.NavigationService.DisplayMessage("Ошибка", responce.ErrorResponse.Message);
                        OnPropertyChanged(nameof(PinCodeCurrentLength));
                        break;
                    default:
                        await ServiceLocator.NavigationService.DisplayMessage("Ошибка", "Получен некорректный ответ");
                        break;
                }
            }
        }
    }
}
