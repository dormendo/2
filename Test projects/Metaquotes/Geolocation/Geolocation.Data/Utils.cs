using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Geolocation.Data
{
	/// <summary>
	/// Статические методы, использующиеся в различных участках кода
	/// </summary>
	internal unsafe static class Utils
	{
		/// <summary>
		/// Копирует данные из одного участка памяти в другой. Применяется для типов указателей
		/// </summary>
		/// <param name="dest">Адрес получателя</param>
		/// <param name="src">Адрес источника</param>
		/// <param name="count">Количество байт, которые необходимо скопировать</param>
		[DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
		internal static extern void CopyMemory(void* dest, void* src, int count);

		/// <summary>
		/// Сравнивает для буфера строк sbyte*
		/// </summary>
		/// <param name="x">Первый буфер</param>
		/// <param name="y">Второй буфер</param>
		/// <param name="length">Длина буферов</param>
		/// <returns>0, если буферы содержат идентичную информацию (до первого \0), -1, если первый буфер "меньше" второго, 1, если первый буфер "больше" второго</returns>
		internal static unsafe int CompareSbpCity(sbyte* x, sbyte* y, int length)
		{
			const sbyte zero = 0;
			for (int i = 0; i < length; i++)
			{
				sbyte xb = *(x + i);
				sbyte yb = *(y + i);
				if (xb < yb)
				{
					return -1;
				}
				if (xb > yb)
				{
					return 1;
				}
				if (xb == zero)
				{
					return 0;
				}
			}

			return 0;
		}

		/// <summary>
		/// Сравнивает для буфера
		/// </summary>
		/// <param name="x">Первый буфер</param>
		/// <param name="y">Второй буфер</param>
		/// <param name="length">Длина буферов</param>
		/// <returns>0, если буферы содержат идентичную информацию, -1, если первый буфер "меньше" второго, 1, если первый буфер "больше" второго</returns>
		internal static unsafe int CompareBuffers(byte* x, byte* y, int length)
		{
			for (int i = 0; i < length; i++)
			{
				byte xb = *(x + i);
				byte yb = *(y + i);
				if (xb < yb)
				{
					return -1;
				}
				if (xb > yb)
				{
					return 1;
				}
			}

			return 0;
		}

		/// <summary>
		/// Преобразует строковое представление IP-адреса в формат БД
		/// </summary>
		/// <param name="ip">Строковое представление IP-адреса</param>
		/// <param name="uintIp">Представление IP-адреса в формате БД</param>
		/// <returns>true, если IP-адрес успешно преобразован, иначе false</returns>
		internal static unsafe bool TryConvertIpToUint(string ip, out uint uintIp)
		{
			uintIp = 0;
			System.Net.IPAddress addr;
			if (System.Net.IPAddress.TryParse(ip, out addr))
			{
				byte[] bytes = addr.GetAddressBytes();
				fixed (uint* uintIpPtr = &uintIp)
				fixed (byte* bp = &bytes[0])
				{
					byte* bIpPtr = (byte*)uintIpPtr;
					bIpPtr[0] = bp[3];
					bIpPtr[1] = bp[2];
					bIpPtr[2] = bp[1];
					bIpPtr[3] = bp[0];
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Преобразует IP-адрес в формате БД в стровое представление
		/// </summary>
		/// <param name="uintIp">Представление IP-адреса в формате БД</param>
		/// <returns>Строковое представление IP-адреса</returns>
		internal static string ConvertUintToIp(uint uintIp)
		{
			uint result = 0;
			uint* uintIpPtr = &uintIp;
			uint* resultPtr = &result;
			byte* bIpPtr = (byte*)uintIpPtr;
			byte* bp = (byte*)resultPtr;
			bp[0] = bIpPtr[3];
			bp[1] = bIpPtr[2];
			bp[2] = bIpPtr[1];
			bp[3] = bIpPtr[0];

			return new System.Net.IPAddress(result).ToString();
		}

		/// <summary>
		/// Диагностический метод
		/// </summary>
		internal static unsafe void ReadCities(Database db)
		{
			int max = 0;
			Dictionary<string, int> dict = new Dictionary<string, int>();
			for (int i = 0; i < db.RecordCount; i++)
			{
				LocationRecord rec = db.GetLocationRecordById(i);
				string strCity = new string(rec.city);
				int count;
				if (!dict.TryGetValue(strCity, out count))
				{
					dict.Add(strCity, 1);
					max = Math.Max(1, max);
				}
				else
				{
					dict[strCity] = count + 1;
					max = Math.Max(count + 1, max);
				}
			}
		}

		/// <summary>
		/// Диагностический метод
		/// </summary>
		internal static unsafe void WriteDbFiles(DataHolder holder, int ipTableSize)
		{
			using (StreamWriter sw = new StreamWriter("ip.txt", false, Encoding.UTF8))
			{
				for (int i = 0; i < ipTableSize; i++)
				{
					IpRecord rec = holder.IpTable[i];
					LocationRecord lrec = holder.GetLocationRecordById((int)rec.location_index);
					string str =
						Utils.ConvertUintToIp(rec.ip_from).PadRight(20) +
						Utils.ConvertUintToIp(rec.ip_to).PadRight(20) +
						new string(lrec.city).PadRight(24);
					sw.WriteLine(str);
				}
			}

			using (StreamWriter sw = new StreamWriter("location.txt", false, Encoding.UTF8))
			{
				for (int i = 0; i < holder.RecordCount; i++)
				{
					LocationRecord rec = holder.GetLocationRecordByNameIndex(i);
					string city = new string(rec.city);
					string country = new string(rec.country);
					string region = new string(rec.region);
					string postal = new string(rec.postal);
					string organization = new string(rec.organization);
					if (city.Length > 23 || country.Length > 7 || region.Length > 11 || postal.Length > 23 || organization.Length > 31)
					{
						throw new Exception();
					}

					string str = city.PadRight(24) +
						country.PadRight(8) +
						region.PadRight(12) +
						postal.PadRight(12) +
						organization.PadRight(32) +
						rec.latitude.ToString().PadRight(20) +
						rec.longitude.ToString().PadRight(20);
					sw.WriteLine(str);
				}
			}
		}

		internal static void ParallelOperation(int threadCount, Action action)
		{
			Task[] tasks = null;
			if (threadCount > 1)
			{
				tasks = new Task[threadCount - 1];
				for (int i = 0; i < threadCount - 1; i++)
				{
					tasks[i] = Task.Run(action);
				}
			}

			action();

			if (threadCount > 1)
			{
				Task.WaitAll(tasks);
			}
		}
	}
}
