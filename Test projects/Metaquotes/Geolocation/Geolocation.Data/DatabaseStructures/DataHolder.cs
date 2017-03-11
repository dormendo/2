namespace Geolocation.Data
{
	internal class DataHolder
	{

		/// <summary>
		/// Заголовок БД
		/// </summary>
		internal DatabaseFileHeader Header;

		/// <summary>
		/// Индекс интервалов IP-адресов
		/// </summary>
		internal IpRecord[] IpTable;

		/// <summary>
		/// Таблица расположений
		/// </summary>
		internal LocationRecord[] LocationTable;

		/// <summary>
		/// Индекс по имени города
		/// </summary>
		internal int[] LocationByNameIndex;

		/// <summary>
		/// Количество записей в БД
		/// </summary>
		internal int RecordCount
		{
			get
			{
				return Header.records;
			}
		}

		/// <summary>
		/// Возвращает идентификатор записи в таблице местоположений по идентификатору записи в индексе, отсортированном по названию города
		/// </summary>
		/// <param name="index">Идентификатор записи в индексе по названию города</param>
		/// <returns>Идентификатор записи в таблице местоположений</returns>
		internal int GetLocationIdByNameIndex(int index)
		{
			return this.LocationByNameIndex[index];
		}

		/// <summary>
		/// Возвращает информацию о местоположении по идентификатору записи в индексе, отсортированном по названию города
		/// </summary>
		/// <param name="index">Идентификатор записи в индексе по названию города</param>
		/// <returns>Информация о местоположении</returns>
		internal LocationRecord GetLocationRecordByNameIndex(int index)
		{
			return this.LocationTable[this.LocationByNameIndex[index]];
		}

		/// <summary>
		/// Возвращает информацию о местоположении по идентификатору записи в таблице
		/// </summary>
		/// <param name="index">Идентификатор записи о местоположении</param>
		/// <returns>Информация о местоположении</returns>
		internal LocationRecord GetLocationRecordById(int locationId)
		{
			return this.LocationTable[locationId];
		}
	}
}
