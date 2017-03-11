using System.IO;

namespace Geolocation.Data
{
	/// <summary>
	/// Полностью считывает файл БД в буфер, из которого затем инициализирует структуры данных
	/// </summary>
	internal class FileStreamLoader : IDbLoader
	{
		/// <summary>
		/// Инициализирует структуры данных информацией из файла БД
		/// </summary>
		/// <param name="fileName">Путь к файлу БД</param>
		/// <param name="holder">Хранилище структур данных БД</param>
		public unsafe void Load(string fileName, DataHolder holder)
		{
			byte[] data = LoadFile(fileName);

			fixed (byte* dataPtr = &data[0])
			{
				fixed (DatabaseFileHeader* headerPtr = &holder.Header)
				{
					Utils.CopyMemory(headerPtr, dataPtr, sizeof(DatabaseFileHeader));
				}

				holder.IpTable = new IpRecord[holder.Header.records];
				holder.LocationTable = new LocationRecord[holder.Header.records];
				holder.LocationByNameIndex = new int[holder.Header.records];

				int blockSize = sizeof(int) * holder.Header.records;
				fixed (int* indexPtr = &holder.LocationByNameIndex[0])
				{
					Utils.CopyMemory(indexPtr, dataPtr + holder.Header.offset_cities, blockSize);
				}

				blockSize = sizeof(LocationRecord) * holder.Header.records;
				fixed (LocationRecord* locationPtr = &holder.LocationTable[0])
				{
					Utils.CopyMemory(locationPtr, dataPtr + holder.Header.offset_locations, blockSize);
				}

				blockSize = sizeof(IpRecord) * holder.Header.records;
				fixed (IpRecord* ipPtr = &holder.IpTable[0])
				{
					Utils.CopyMemory(ipPtr, dataPtr + holder.Header.offset_ranges, blockSize);
				}
			}
		}

		private byte[] LoadFile(string fileName)
		{
			using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read,
				FileShare.None, 128 * 1024, FileOptions.SequentialScan))
			{
				byte[] buffer = new byte[fs.Length];
				int bytesReadTotal = 0;
				while (true)
				{
					int bytesRead = fs.Read(buffer, bytesReadTotal, (int)fs.Length - bytesReadTotal);
					bytesReadTotal += bytesRead;
					if (bytesRead == 0 || bytesReadTotal == fs.Length)
					{
						break;
					}
				}

				return buffer;
			}
		}
	}
}
