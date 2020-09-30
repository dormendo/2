using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Tips24.Client.CustomViews
{
    public partial class SteppedSlider : ContentView
    {
        public static readonly BindableProperty MinimumProperty =
            BindableProperty.Create("Minimum", typeof(int), typeof(SteppedSlider), 0, propertyChanged: OnMinimumChanged);

        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly BindableProperty MaximumProperty =
            BindableProperty.Create("Maximum", typeof(int), typeof(SteppedSlider), 10, propertyChanged: OnMaximumChanged);

        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }


        public static readonly BindableProperty StepProperty =
            BindableProperty.Create("Step", typeof(int), typeof(SteppedSlider), 1);

        public int Step
        {
            get { return (int)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }

        public static readonly BindableProperty ValueProperty =
            BindableProperty.Create("Value", typeof(int), typeof(SteppedSlider), 5,defaultBindingMode: BindingMode.TwoWay);

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public SteppedSlider()
        {
            InitializeComponent();

            _slider.ValueChanged += OnSliderValueChanged;
        }

        private void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            Value = (int)(_slider.Value / Step) * Step;
        }

        private static void OnStepChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var steppedSlider = (SteppedSlider)bindable;

            RefreshInnerSlider(steppedSlider);
        }

        private static void OnMinimumChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var steppedSlider = (SteppedSlider)bindable;
            RefreshInnerSlider(steppedSlider);
        }

        private static void OnMaximumChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var steppedSlider = (SteppedSlider)bindable;
            RefreshInnerSlider(steppedSlider);
        }

        private static void RefreshInnerSlider(SteppedSlider steppedSlider)
        {
            var innerSlider = steppedSlider._slider;

            innerSlider.Minimum = steppedSlider.Minimum;
            innerSlider.Maximum = steppedSlider.Maximum;
        }
    }
}