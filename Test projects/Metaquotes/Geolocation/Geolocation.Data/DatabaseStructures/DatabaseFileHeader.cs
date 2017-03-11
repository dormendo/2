using System.Runtime.InteropServices;

namespace Geolocation.Data
{
	/// <summary>
	/// Заголовок БД
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal unsafe struct DatabaseFileHeader
	{
		internal int version;
		internal fixed sbyte name[32];
		internal ulong timestamp;
		internal int records;
		internal uint offset_ranges;
		internal uint offset_cities;
		internal uint offset_locations;
	}
}
