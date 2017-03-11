using System.Runtime.InteropServices;

namespace Geolocation.Data
{
	/// <summary>
	/// Запись индекса интервалов IP
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	internal unsafe struct IpRecord
	{
		internal uint ip_from;
		internal uint ip_to;
		internal uint location_index;
	}
}
