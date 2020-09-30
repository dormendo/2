using System;
using System.Collections.Generic;
using Tips24.Client.ViewModels;
using Xamarin.Forms;

namespace Tips24.Client.Pages
{
    public partial class PhoneNumberLoginPage : ContentPage
    {
        public PhoneNumberLoginPage()
        {
            InitializeComponent();
        }

        public PhoneNumberLoginPage(PhoneNumberLoginViewModel viewModel) : this()
        {
            BindingContext = viewModel;
        }
    }
}
