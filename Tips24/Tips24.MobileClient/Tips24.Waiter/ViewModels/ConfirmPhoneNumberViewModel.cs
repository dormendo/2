using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Tips24.Client.Model;
using Tips24.Client.Services;
using Tips24.Client.ViewModels;
using Xamarin.Forms;

namespace Tips24.Client.ViewModels
{
    public class ConfirmPhoneNumberViewModel : BaseViewModel, IInitializable
    {
        private static readonly TimeSpan RepeatSendingInterval = TimeSpan.FromSeconds(45);
        private readonly WaiterRegistrationContext _context;
        private Guid _currentVerificationId;
        private int _repeateSendingCountdown;
        private string _smsCode;

        public ConfirmPhoneNumberViewModel(WaiterRegistrationContext context)
        {
            CheckVerificationCodeCommand = new Command(CheckVerificationCode, CanCheckVerificationCode);
            SendVerificationCodeCommand = new Command(async () => await SendVerificationCode(), CanSendVerificationCode);
            _context = context;
        }

        public string SmsCode
        {
            get => _smsCode; 
            set
            {
                _smsCode = value;
                OnPropertyChanged();
                ((Command)CheckVerificationCodeCommand).ChangeCanExecute();
            }
        }

        public int SendVerificationCodeCountdown
        {
            get => _repeateSendingCountdown;
            set
            {
                _repeateSendingCountdown = value;
                OnPropertyChanged();
                ((Command)CheckVerificationCodeCommand).ChangeCanExecute();
            }
        }

        public ICommand CheckVerificationCodeCommand { get; }

        public ICommand SendVerificationCodeCommand { get; }


        public void Initialize()
        {
            SendVerificationCode();
        }

        private async void RunCountdown()
        {
            var sendVerificationCodeTime = DateTime.Now;

            while (DateTime.Now < sendVerificationCodeTime + RepeatSendingInterval)
            {
                SendVerificationCodeCountdown = (RepeatSendingInterval - (DateTime.Now - sendVerificationCodeTime)).Seconds;
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            SendVerificationCodeCountdown = 0;
        }

        private async void CheckVerificationCode()
        {
            if(!string.IsNullOrWhiteSpace(SmsCode))
            {
                var responce = await ServiceLocator.Api.Auth.CheckVerificationCode(
                    new Dto.Auth.CheckVcodeRequest
                    {
                        Code = SmsCode,
                        VerificationId = _currentVerificationId
                    });

                switch (responce.Kind)
                {
                    case Services.Api.ApiResponseKind.Success:
                        await ServiceLocator.NavigationService.NavigateTo(this, new RegistrationViewModel(_context));
                        break;
                    case Services.Api.ApiResponseKind.Error:
                        await ServiceLocator.NavigationService.DisplayMessage("Ошибка", responce.ErrorResponse.Message);
                        break;
                    default:
                        await ServiceLocator.NavigationService.DisplayMessage("Ошибка", "Получен некорректный статус");
                        break;
                }
            }

            SmsCode = string.Empty;
        }

        private async Task SendVerificationCode()
        {
            RunCountdown();

            var responce = await ServiceLocator.Api.Auth.SendVerificationCode(
                new Dto.Auth.SendVcodeRequest
                {
                    Phone = _context.PhoneNumber
                });

            switch (responce.Kind)
            {
                case Services.Api.ApiResponseKind.Success:
                    _currentVerificationId = responce.Response.VerificationId;
                    break;
                case Services.Api.ApiResponseKind.Error:
                    await ServiceLocator.NavigationService.DisplayMessage("Ошибка", responce.ErrorResponse.Message);
                    break;
                default:
                    await ServiceLocator.NavigationService.DisplayMessage("Ошибка", "Получен некорректный статус");
                    break;
            }
        }

        private bool CanCheckVerificationCode()
        {
            return !string.IsNullOrWhiteSpace(SmsCode);
        }

        private bool CanSendVerificationCode()
        {
            return SendVerificationCodeCountdown == 0;
        }
    }
}
