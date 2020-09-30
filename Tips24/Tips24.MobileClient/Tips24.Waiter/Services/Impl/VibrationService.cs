using System;
using Xamarin.Essentials;

namespace Tips24.Client.Services.Impl
{
    public class VibrationService : IVibrationService
    {
        public void Vibrate()
        {
            try
            {
                var duration = TimeSpan.FromMilliseconds(500);
                Vibration.Vibrate(duration);
            }
            catch (FeatureNotSupportedException ex)
            {
                // Feature not supported on device
                Logger.Error(ex);
            }
        }
    }
}
