using System;
using System.Collections.Generic;
using Tips24.Client.ViewModels;
using Xamarin.Forms;

namespace Tips24.Client.Pages
{
    public partial class OperationsPage : TabbedPage
    {
        public OperationsPage()
        {
            InitializeComponent();
        }

        public OperationsPage(OperationsViewModel operationsViewModel) : this()
        {
            BindingContext = operationsViewModel;
        }
    }
}
