using System;
namespace Tips24.Client.Model
{
    public class WaiterRegistrationContext
    {
        public WaiterRegistrationContext(string placeName, string placeAddress, int placeToJoinId, Guid linkParameter)
        {
            PlaceName = placeName;
            PlaceAddress = placeAddress;
            PlaceToJoinId = placeToJoinId;
            LinkParameter = linkParameter;

            PinCode = string.Empty;
        }

        public string Name { get; set; }

        public string Surname { get; set; }

        public string PhoneNumber { get; set; }

        public string PlaceName { get; }

        public string PlaceAddress { get; }

        public int PlaceToJoinId { get; }

        public Guid LinkParameter { get; }

        public string PinCode { get; set; }
    }
}
