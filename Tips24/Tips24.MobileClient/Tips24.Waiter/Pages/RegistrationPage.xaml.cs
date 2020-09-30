using System;
using System.Collections.Generic;
using Tips24.Client.ViewModels;
using Xamarin.Forms;

namespace Tips24.Client.Pages
{
    public partial class RegistrationPage : ContentPage
    {
        public RegistrationPage()
        {
            InitializeComponent();
        }

        public RegistrationPage(RegistrationViewModel viewModel) : this()
        {
            BindingContext = viewModel;
        }
    }
}
