using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Tips24.Client.CustomViews
{
    public partial class PinCodeKeyboard : ContentView
    {
        public static readonly BindableProperty PinCodeMaxSizeProperty =
            BindableProperty.Create("PinCodeMaxSize", typeof(int), typeof(SteppedSlider), 4);

        public int PinCodeMaxSize
        {
            get { return (int)GetValue(PinCodeMaxSizeProperty); }
            set { SetValue(PinCodeMaxSizeProperty, value); }
        }

        public static readonly BindableProperty PinCodeProperty =
            BindableProperty.Create("PinCode", typeof(string), typeof(SteppedSlider), string.Empty, propertyChanged: OnPropertyChanged);

        private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue == null)
            {
                ((PinCodeKeyboard)bindable).PinCode = string.Empty;
            }
        }

        public string PinCode
        {
            get { return (string)GetValue(PinCodeProperty); }
            set { SetValue(PinCodeProperty, value); }
        }

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create("Command", typeof(ICommand), typeof(SteppedSlider));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public PinCodeKeyboard()
        {
            InitializeComponent();
            UpdateRemoveNumberBtnVisibility();
        }

        private async void OnInputNumberClicked(object sender, System.EventArgs e)
        {
            var number = ((Button)sender).Text;
            if (PinCode.Length < PinCodeMaxSize)
            {
                PinCode = PinCode + number;
                UpdateRemoveNumberBtnVisibility();
            }

            if (PinCode.Length == PinCodeMaxSize)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                if (Command != null)
                {
                    Command.Execute(null);
                }
            }
        }

        private void OnRemoveNumberClicked(object sender, System.EventArgs e)
        {
            if (PinCode.Length > 0)
            {
                PinCode = PinCode.Substring(0, PinCode.Length - 1);
                UpdateRemoveNumberBtnVisibility();
            }
        }

        private void UpdateRemoveNumberBtnVisibility()
        {
            _removeNumberBtn.IsVisible = PinCode.Length > 0;
        }
    }
}