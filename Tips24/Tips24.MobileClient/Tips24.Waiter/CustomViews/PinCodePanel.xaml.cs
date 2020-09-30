using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Tips24.Client.CustomViews
{
    public partial class PinCodePanel : ContentView
    {
        public static readonly BindableProperty IndicatorsCountProperty =
            BindableProperty.Create("IndicatorsCount", typeof(int), typeof(PinCodePanel), 6);

        public int IndicatorsCount
        {
            get { return (int)GetValue(IndicatorsCountProperty); }
            set { SetValue(IndicatorsCountProperty, value); }
        }

        public static readonly BindableProperty ActiveIndicatorsCountProperty =
            BindableProperty.Create("ActiveIndicatorsCount", typeof(int), typeof(PinCodePanel), 0, propertyChanged: OnActiveIndicatorsCountChanged);



        public int ActiveIndicatorsCount
        {
            get { return (int)GetValue(ActiveIndicatorsCountProperty); }
            set { SetValue(ActiveIndicatorsCountProperty, value); }
        }

        public PinCodePanel()
        {
            InitializeComponent();
        }


        private static void OnActiveIndicatorsCountChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((PinCodePanel)bindable)._canvasView.InvalidateSurface();
        }

        //public void IncorrectSignal()
        //{

        //}

        private void OnCanvasViewPaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            var sectionWidth = info.Width / IndicatorsCount;
            var centers = new List<SKPoint>();

            for (int i = 0; i < IndicatorsCount; i++)
            {
                centers.Add(new SKPoint(sectionWidth * i + sectionWidth / 2, info.Height / 2));
            }

            float radius = Math.Min(sectionWidth, info.Height) * 0.2f;
  
            for (int i = 0; i < IndicatorsCount; i++)
            {
                using(var curIndicator = new SKPaint())
                {
                    var curCenter = centers[i];

                    curIndicator.StrokeWidth = radius / 2;
                    curIndicator.IsAntialias = true;

                    curIndicator.Style = SKPaintStyle.StrokeAndFill;
                    curIndicator.Color = SKColors.Gray;
                    canvas.DrawOval(curCenter.X, curCenter.Y, radius, radius, curIndicator);

                    if (ActiveIndicatorsCount > i)
                    {
                        curIndicator.Style = SKPaintStyle.Fill;
                        curIndicator.Color = SKColors.White;
                        canvas.DrawOval(curCenter.X, curCenter.Y, radius, radius, curIndicator);
                    }
                    else
                    {
                        curIndicator.Style = SKPaintStyle.Fill;
                        curIndicator.Color = SKColors.LightGray;
                        canvas.DrawOval(curCenter.X, curCenter.Y, radius, radius, curIndicator);
                    }
                }
            }
        }
    }
}
