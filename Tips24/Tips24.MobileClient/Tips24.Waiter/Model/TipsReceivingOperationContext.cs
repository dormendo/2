using System;
namespace Tips24.Client.Model
{
    public class TipsReceivingOperationContext
    {
        public TipsReceivingOperationContext(string placeName)
        {
            PlaceName = placeName;
        }

        public decimal Amount { get; set; }

        public TipsReceivingMode Mode { get; set; }

        public string PlaceName { get; set; }

        public BankInfo Bank { get; set; }

        /// <summary>
        /// Номер телефона, на который будет выслано sms-соощение для поддтерждения платежа.
        /// </summary>
        public string PhoneNumber { get; set; }
    }
}
