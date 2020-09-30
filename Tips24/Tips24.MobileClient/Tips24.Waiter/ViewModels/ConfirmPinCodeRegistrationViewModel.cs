using System;
using System.Windows.Input;
using Tips24.Client.Model;
using Tips24.Client.Services;
using Xamarin.Forms;

namespace Tips24.Client.ViewModels
{
    public class ConfirmPinCodeRegistrationViewModel : BaseViewModel
    {
        private readonly WaiterRegistrationContext _context;
        private string _pinCode = string.Empty;

        public ConfirmPinCodeRegistrationViewModel(WaiterRegistrationContext context)
        {
            _context = context;
            PinCodeSize = 4;

            ReturnCommand = new Command(Continue);
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

        private async void Continue()
        {
            if (PinCode != _context.PinCode)
            {
                PinCode = string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(PinCode))
            {
                var responce = await ServiceLocator.Api.Auth.Register(new Dto.Auth.RegisterRequest
                {
                    FirstName = _context.Name,
                    LastName = _context.Surname,
                    LinkParameter = _context.LinkParameter,
                    Phone = _context.PhoneNumber,
                    PinCode = _context.PinCode,
                    PlaceToJoinId = _context.PlaceToJoinId
                });

                switch (responce.Kind)
                {
                    case Services.Api.ApiResponseKind.Success:
                        ServiceLocator.AuthorizationService.SetRegularAuthKey(responce.Response.PermanentKey);
                        await ServiceLocator.NavigationService.NavigateTo(this, new OperationsViewModel());
                        break;
                    case Services.Api.ApiResponseKind.Error:
                        //todo скорее всего не все ошибки стоит обрабатывать таким образом
                        await ServiceLocator.NavigationService.DisplayMessage("Ошибка", responce.ErrorResponse.Message);
                        break;
                    default:
                        await ServiceLocator.NavigationService.DisplayMessage("Ошибка", "Получен некорректный ответ");
                        break;
                }
            }
        }
    }
}
