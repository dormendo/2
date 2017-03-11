using System.IO;
using System.IO.MemoryMappedFiles;

namespace Geolocation.Data
{
	/// <summary>
	/// Инициализирует структуры данных с использованием файла, отображённого в память.
	/// Наиболее простая и самая медленная версия загрузчика
	/// </summary>
	internal class MemoryMappedFileLoader : IDbLoader
	{
		/// <summary>
		/// Инициализирует структуры данных информацией из файла БД
		/// </summary>
		/// <param name="fileName">Путь к файлу БД</param>
		/// <param name="holder">Хранилище структур данных БД</param>
		public void Load(string fileName, DataHolder holder)
		{
			using (MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(fileName, FileMode.Open, "a", 0, MemoryMappedFileAccess.Read))
			{
				using (MemoryMappedViewAccessor mmfa = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read))
				{

					mmfa.Read(0, out holder.Header);

					holder.IpTable = new IpRecord[holder.Header.records];
					holder.LocationTable = new LocationRecord[holder.Header.records];
					holder.LocationByNameIndex = new int[holder.Header.records];

					mmfa.ReadArray(holder.Header.offset_ranges, holder.IpTable, 0, holder.Header.records);
					mmfa.ReadArray(holder.Header.offset_locations, holder.LocationTable, 0, holder.Header.records);
					mmfa.ReadArray(holder.Header.offset_cities, holder.LocationByNameIndex, 0, holder.Header.records);
				}
			}
		}
	}
}
