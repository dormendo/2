using System;
using System.Collections.Generic;
using Tips24.Client.ViewModels;
using Xamarin.Forms;

namespace Tips24.Client.Pages
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        public LoginPage(LoginViewModel viewModel) : this()
        {
            BindingContext = viewModel;
        }
    }
}
