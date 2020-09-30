using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tips24.Client.Pages;
using Tips24.Client.ViewModels;
using Xamarin.Forms;

namespace Tips24.Client.Services
{
    public class NavigationService
    {
		public string LaunchingLink { get; private set; }
		public void Initialize(string launchingLink)
        {
			this.LaunchingLink = launchingLink;
			MessagingCenter.Subscribe<NavigationService, NavigationContext>(this, Events.NavigateToEvent, OnNavigateTo);
            MessagingCenter.Subscribe<NavigationService, NavigationContext>(this, Events.NavigateBackEvent, OnNavigateBack);
            MessagingCenter.Subscribe<NavigationService, DisplayAlertContext>(this, Events.DisplayAlertEvent, OnDisplayAlert);
        }

        public class DisplayAlertContext
        {
            public string Message { get; private set; }
            public string Title { get; private set; }
            public string OK { get; private set; }

            public TaskCompletionSource<object> Awaiter { get; private set; }

            public DisplayAlertContext(string title, string message, string ok, TaskCompletionSource<object> awaiter)
            {
                Title = title;
                Message = message;
                OK = ok;

                Awaiter = awaiter;
            }
        }

        public class NavigationContext
        {
            public BaseViewModel From { get; private set; }

            public BaseViewModel To { get; private set; }
           
            public Type ToType { get; private set; }

            public TaskCompletionSource<object> Awaiter { get; private set; }


            public NavigationContext(BaseViewModel from, BaseViewModel to, TaskCompletionSource<object> awaiter)
            {
                From = from;
                To = to;
                Awaiter = awaiter;
            }

            public NavigationContext(BaseViewModel from, TaskCompletionSource<object> awaiter)
            {
                From = from;
                Awaiter = awaiter;
            }

            public NavigationContext(BaseViewModel from, Type toType, TaskCompletionSource<object> awaiter)
            {
                From = from;
                ToType = toType;
                Awaiter = awaiter;
            }
        }

        public Task DisplayMessage(string title, string message, string ok = "OK")
        {
            TaskCompletionSource<object> awaiter = new TaskCompletionSource<object>();
            MessagingCenter.Send(this, Events.DisplayAlertEvent, new DisplayAlertContext(title, message, ok, awaiter));
            return awaiter.Task;
        }

        public Task NavigateTo(BaseViewModel from, BaseViewModel to)
        {
            TaskCompletionSource<object> awaiter = new TaskCompletionSource<object>();
            MessagingCenter.Send(this, Events.NavigateToEvent, new NavigationContext(from, to, awaiter));
            return awaiter.Task;
        }

        public Task NavigateBack(BaseViewModel from)
        {
            TaskCompletionSource<object> awaiter = new TaskCompletionSource<object>();
            MessagingCenter.Send(this, Events.NavigateBackEvent, new NavigationContext(from, awaiter));
            return awaiter.Task;
        }

        public Task NavigateBack<TTarget>(BaseViewModel from)
        {
            TaskCompletionSource<object> awaiter = new TaskCompletionSource<object>();
            MessagingCenter.Send(this, Events.NavigateBackEvent, new NavigationContext(from, typeof(TTarget), awaiter));
            return awaiter.Task;
        }

        private async void OnDisplayAlert(NavigationService navigationService, DisplayAlertContext context)
        {
            var navigation = Application.Current.MainPage.Navigation;
            await navigation.NavigationStack.Last().DisplayAlert(context.Title, context.Message, context.OK);
            context.Awaiter.SetResult(null);
        }

        private async void OnNavigateBack(NavigationService navigationService, NavigationContext navigationContext)
        {
            try
            {
                TaskCompletionSource<object> viewPushedAwaiter = new TaskCompletionSource<object>();
                var navigation = Application.Current.MainPage.Navigation;

                if (navigationContext.ToType == null)
                {
                    await navigation.PopAsync();
                }
                else
                {
                    if (navigation.NavigationStack.Any(p => p.BindingContext.GetType() == navigationContext.ToType))
                    {
                        var pagesToRemoveCount = navigation
                            .NavigationStack
                            .Reverse()
                            .TakeWhile(p => p.BindingContext.GetType() != navigationContext.ToType)
                            .Count();

                        for (int i = 1; i < pagesToRemoveCount; i++)
                        {
                            navigation.RemovePage(navigation.NavigationStack[navigation.NavigationStack.Count - 2]);
                        }

                        await navigation.PopAsync();
                    }
                    else
                    {
                        //todo залогировать
                    }
                }

                navigationContext.Awaiter.SetResult(null);
            }
            catch (Exception)
            {
                //todo залогировать
                throw;
            }
        }

        private async void OnNavigateTo(NavigationService navigationService, NavigationContext navigationContext)
        {
            try
            {
                TaskCompletionSource<object> viewPushedAwaiter = new TaskCompletionSource<object>();

                var navigation = Application.Current.MainPage.Navigation;

                switch (navigationContext.To)
                {
                    case OperationsViewModel operationsViewModel:
                        // окно открывается либо первым, либо после авторизации, либо после регистрации

                        var pagesToRemove = new List<Page>(navigation.NavigationStack);

                        await navigation.PushAsync(new OperationsPage
                        {
                            BindingContext = navigationContext.To
                        });

                        foreach (var pageToRemove in pagesToRemove)
                        {
                            navigation.RemovePage(pageToRemove);
                        }
                        break;

                    case SettingsViewModel settingsViewModel:
                        await navigation.PushAsync(new SettingsPage
                        {
                            BindingContext = navigationContext.To
                        });
                        break;

                    case InputTipsAmountViewModel inputTipsAmountViewModel:

                        await navigation.PushAsync(new InputTipsAmountPage
                        {
                            BindingContext = navigationContext.To
                        });
                        break;

                    case SelectTipsModeViewModel selectTipsModeViewModel:

                        await navigation.PushAsync(new SelectTipsModePage
                        {
                            BindingContext = navigationContext.To
                        });
                        break;

                    case SelectBankViewModel selectBankViewModel:

                        await navigation.PushAsync(new SelectBankPage
                        {
                            BindingContext = navigationContext.To
                        });
                        break;

                    case QRCodeViewModel qrCodeViewModel:

                        await navigation.PushAsync(new QRCodePage
                        {
                            BindingContext = navigationContext.To
                        });
                        break;

                    case InputPnoneNumberSmsBankViewModel inputPnoneNumberSmsBankViewModel:

                        await navigation.PushAsync(new InputPnoneNumberSmsBankPage
                        {
                            BindingContext = navigationContext.To
                        });
                        break;

                    case ConfirmSmsBankViewModel confirmSmsBankViewModel:

                        await navigation.PushAsync(new ConfirmSmsBankPage
                        {
                            BindingContext = navigationContext.To
                        });
                        break;

                    case ConfirmPhoneNumberViewModel confirmPhoneNumberViewModel:

                        await navigation.PushAsync(new ConfirmPhoneNumberPage
                        {
                            BindingContext = navigationContext.To
                        });
                        break;

                    case RegistrationViewModel registrationViewModel:

                        await navigation.PushAsync(new RegistrationPage
                        {
                            BindingContext = navigationContext.To
                        });
                        break;

                    case CreatePinCodeRegistrationViewModel createPinCodeRegistrationViewModel:

                        await navigation.PushAsync(new CreatePinCodeRegistrationPage
                        {
                            BindingContext = navigationContext.To
                        });
                        break;

                    case ConfirmPinCodeRegistrationViewModel confirmPinCodeRegistrationViewModel:

                        await navigation.PushAsync(new ConfirmPinCodeRegistrationPage
                        {
                            BindingContext = navigationContext.To
                        });
                        break;

                    case LoginViewModel loginViewModel:

                        await navigation.PushAsync(new ConfirmPinCodeRegistrationPage
                        {
                            BindingContext = navigationContext.To
                        });
                        break;
                }

                if (navigationContext.To is IInitializable initializableViewModel)
                {
                    initializableViewModel.Initialize();
                }

                navigationContext.Awaiter.SetResult(null);
            }
            catch (Exception e)
            {
                //todo залогировать
                throw;
            }
        }
    }
}
