using System;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Tips24.Client.Model;
using Tips24.Client.Services;
using Xamarin.Forms;

namespace Tips24.Client.ViewModels
{
    public class InputPnoneNumberSmsBankViewModel : BaseViewModel
    {
        private const string PhoneNumberMask = "+7 XXX XXX XX XX";

        private readonly TipsReceivingOperationContext _tipsReceivingOperationContext;
        private string _phoneNumber = string.Empty;

        public ICommand InputNumberCommand { get; }

        public ICommand RemoveNumberCommand { get; }

        public ICommand ContinueCommand { get; }

        public string OnlineBankServiceName
        {
            get { return _tipsReceivingOperationContext.Bank.OnlineServiceName; }
        }

        public string FormattedPhoneNumber 
        {
            get
            {
                var formattedPhoneNumber = new StringBuilder();
                var curPos = 0;

                foreach (var ch in PhoneNumberMask)
                {
                    if (ch != 'X')
                    {
                        formattedPhoneNumber.Append(ch);
                    }
                    else
                    {
                        if (curPos < _phoneNumber.Length)
                        {
                            formattedPhoneNumber.Append(_phoneNumber[curPos]);
                            curPos++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return formattedPhoneNumber.ToString();
            }
        }

        public InputPnoneNumberSmsBankViewModel(TipsReceivingOperationContext tipsReceivingOperationContext)
        {
            _tipsReceivingOperationContext = tipsReceivingOperationContext;

            InputNumberCommand = new Command<string>(InputNumber);

            RemoveNumberCommand = new Command(RemoveNumber);

            ContinueCommand = new Command(Continue, CanContinue);
        }

        private void Continue()
        {
            if (CanContinue())
            {
                _tipsReceivingOperationContext.PhoneNumber = FormattedPhoneNumber;
                ServiceLocator.NavigationService.NavigateTo(this, new ConfirmSmsBankViewModel(_tipsReceivingOperationContext));
            }
        }

        private bool CanContinue()
        {
            return PhoneNumberMask.Count(c => c == 'X') == _phoneNumber.Length;
        }

        private void InputNumber(string number)
        {
            if (_phoneNumber.Length < PhoneNumberMask.Count(c => c == 'X'))
            {
                _phoneNumber = _phoneNumber + int.Parse(number);
                OnPropertyChanged(nameof(FormattedPhoneNumber));
                ((Command)ContinueCommand).ChangeCanExecute();
            }
        }

        private void RemoveNumber()
        {
            if (_phoneNumber.Length > 0)
            {
                _phoneNumber = _phoneNumber.Substring(0, _phoneNumber.Length - 1);
                OnPropertyChanged(nameof(FormattedPhoneNumber));
                ((Command)ContinueCommand).ChangeCanExecute();
            }
        }
    }
}
