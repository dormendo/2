using System;
using Tips24.Client.ViewModels;
using Xamarin.Forms;

namespace Tips24.Client
{
    public class OperationTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SelectTipsModeTemplate { get; set; }
       
        public DataTemplate BalanceTemplate { get; set; }
       
        public DataTemplate HelpTemplate { get; set; }

        public DataTemplate InfoTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is SelectTipsModeViewModel)
                return SelectTipsModeTemplate;

            if (item is BalanceViewModel)
                return BalanceTemplate;

            if (item is HelpViewModel)
                return HelpTemplate;

            if (item is InfoViewModel)
                return InfoTemplate;

            throw new ArgumentException();
        }
    }
}
