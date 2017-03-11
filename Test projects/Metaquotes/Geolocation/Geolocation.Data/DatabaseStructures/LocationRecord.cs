using System.Runtime.InteropServices;

namespace Geolocation.Data
{
	/// <summary>
	/// Запись таблицы расположений
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal unsafe struct LocationRecord
	{
		internal fixed sbyte country[8];
		internal fixed sbyte region[12];
		internal fixed sbyte postal[12];
		internal fixed sbyte city[24];
		internal fixed sbyte organization[32];
		internal float latitude;
		internal float longitude;
	}
}
