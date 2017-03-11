using System;
using System.Collections.Generic;
using System.Text;

namespace Geolocation.Data
{
	/// <summary>
	/// База данных
	/// </summary>
	internal unsafe class Database
	{
		/// <summary>
		/// Фактический предел длины для названия города
		/// </summary>
		private const int MAX_CITY_LENGTH = 23;

		/// <summary>
		/// Путь к файлу базы данных
		/// </summary>
		private string _fileName;

		/// <summary>
		/// Структура, хранящая данные, считанные из файла БД
		/// </summary>
		private DataHolder _holder;

		/// <summary>
		/// Фактический размер индекса IP-адресов.
		/// Тестовая БД содержит неконсистентные данные в конце индекса, которые необходимо отсечь из обработки
		/// </summary>
		private int _ipTableSize;

		/// <summary>
		/// Количество записей о расположениях в БД
		/// </summary>
		internal int RecordCount
		{
			get { return _holder.RecordCount; }
		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="fileName">Путь к файлу БД</param>
		internal Database(string fileName)
		{
			_fileName = fileName;
		}

		/// <summary>
		/// Инициализирует БД
		/// </summary>
		internal void Initialize()
		{
			this.AcquireTables();

			this.CheckIpTable();
			//Utils.WriteDbFiles(_holder, RecordCount);
		}

		/// <summary>
		/// Возвращает идентификатор записи в таблице местоположений по идентификатору записи в индексе, отсортированном по названию города
		/// </summary>
		/// <param name="index">Идентификатор записи в индексе по названию города</param>
		/// <returns>Идентификатор записи в таблице местоположений</returns>
		internal int GetLocationIdByNameIndex(int index)
		{
			return this._holder.GetLocationIdByNameIndex(index);
		}

		/// <summary>
		/// Возвращает информацию о местоположении по идентификатору записи в индексе, отсортированном по названию города
		/// </summary>
		/// <param name="index">Идентификатор записи в индексе по названию города</param>
		/// <returns>Информация о местоположении</returns>
		internal LocationRecord GetLocationRecordByNameIndex(int index)
		{
			return this._holder.GetLocationRecordByNameIndex(index);
		}

		/// <summary>
		/// Возвращает информацию о местоположении по идентификатору записи в таблице
		/// </summary>
		/// <param name="index">Идентификатор записи о местоположении</param>
		/// <returns>Информация о местоположении</returns>
		internal LocationRecord GetLocationRecordById(int locationId)
		{
			return this._holder.GetLocationRecordById(locationId);
		}

		/// <summary>
		/// Таблица IP-адресов содержит коллизию, начиная с записи №99038 (по счёту с 0).
		/// Данный метод вычисляет последний корректный интервал IP-адресов в данной структуре.
		/// В качестве варианта можно вводить это значение в виде настройки
		/// </summary>
		private void CheckIpTable()
		{
			uint prevValue = 0;
			for (int i = 0; i < RecordCount; i++)
			{
				IpRecord ipRec = _holder.IpTable[i];
				uint curValue = ipRec.ip_from;
				if (ipRec.ip_from < prevValue || ipRec.ip_to < ipRec.ip_from)
				{
					_ipTableSize = i;
					return;
				}

				prevValue = ipRec.ip_from;
			}

			_ipTableSize = RecordCount;
		}

		/// <summary>
		/// Загружает данные из файла БД
		/// Используется наиболее производительный вариант загрузчика без IoC и возможности подмены
		/// </summary>
		private unsafe void AcquireTables()
		{
			_holder = new DataHolder();
			IDbLoader loader = new FileStreamLoader2();
			loader.Load(_fileName, _holder);
		}

		/// <summary>
		/// Возвращает идентификатор местоположения по ip-адресу
		/// </summary>
		/// <param name="ip">IP-адрес</param>
		/// <param name="locationIndex">Идентификатор местоположения</param>
		/// <returns>true, если местоположение найдено. Иначе false</returns>
		internal bool GetLocationIndexByIp(uint ip, out uint locationIndex)
		{
			int index = Array.BinarySearch(_holder.IpTable, 0, _ipTableSize, new IpRecord { ip_from = ip }, IpRecordComparer.Instance);

			locationIndex = 0;
			bool locationFound = false;
			if (index >= 0)
			{
				if (index < _ipTableSize)
				{
					locationIndex = _holder.IpTable[index].location_index;
					locationFound = true;
				}
			}
			else if (index < 0)
			{
				index = ~index;
				if (index > 0 && index <= _ipTableSize)
				{
					IpRecord ipRange = _holder.IpTable[index - 1];
					if (ipRange.ip_from <= ip && ipRange.ip_to >= ip)
					{
						locationIndex = ipRange.location_index;
						locationFound = true;
					}
				}
			}

			return locationFound;
		}

		/// <summary>
		/// Возвращает интервал идентификаторов в сортированном индексе по названию города 
		/// </summary>
		/// <param name="city">Название города</param>
		/// <param name="firstElement">Первый идентификатор интервала</param>
		/// <param name="lastElement">Последний идентификатор интервала</param>
		/// <returns>true, если город найден. Иначе false</returns>
		internal unsafe bool GetLocationsByCityName(string city, out int firstElement, out int lastElement)
		{
			firstElement = -1;
			lastElement = -1;

			if (city == null || city.Length == 0 || city.Length > MAX_CITY_LENGTH)
			{
				return false;
			}

			byte[] bCity = new byte[24];
			Encoding.Default.GetBytes(city, 0, city.Length, bCity, 0);

			fixed (byte* bpCity = &bCity[0])
			{
				sbyte* sbpCity = (sbyte*)bpCity;

				int rangeStart = 0;
				int rangeEnd = _holder.LocationByNameIndex.Length - 1;
				int foundElement = this.BinarySearchByCityName(sbpCity, ref rangeStart, ref rangeEnd);

				if (foundElement == -1)
				{
					return false;
				}

				if (rangeStart < foundElement)
				{
					firstElement = FindEdgeForCity(sbpCity, rangeStart, foundElement - 1, true);
				}

				if (foundElement < rangeEnd)
				{
					lastElement = FindEdgeForCity(sbpCity, foundElement + 1, rangeEnd, false);
				}

				firstElement = (firstElement == -1 ? foundElement : firstElement);
				lastElement = (lastElement == -1 ? foundElement : lastElement);

				return true;
			}
		}

		/// <summary>
		/// Если город был найден процедурой двоичного поиска,
		/// в возвращённом ею интервале записей нужно найти первый и последний элементы интервала, соответствующего городу
		/// </summary>
		/// <param name="sbpCity">Название города</param>
		/// <param name="rangeStart">
		/// Принимает на вход идентификатор первого элемента интервала, в котором нужно осуществлять поиск.
		/// Возвращает идентификатор первого элемента интервала, соответствующего городу
		/// </param>
		/// <param name="rangeEnd">
		/// Принимает на вход идентификатор последнего элемента интервала, в котором нужно осуществлять поиск.
		/// Возвращает идентификатор последнего элемента интервала, соответствующего городу
		/// </param>
		/// <param name="leftEdge">true, если необходимо найти первый элемента интервала. false, если последний</param>
		/// <returns></returns>
		private unsafe int FindEdgeForCity(sbyte* sbpCity, int rangeStart, int rangeEnd, bool leftEdge)
		{
			int foundElement = -1, lastFoundElement = -1;
			while (true)
			{
				foundElement = this.BinarySearchByCityName(sbpCity, ref rangeStart, ref rangeEnd);

				if (foundElement == -1)
				{
					foundElement = lastFoundElement;
					break;
				}

				lastFoundElement = foundElement;
				if (rangeStart == rangeEnd)
				{
					break;
				}

				if (leftEdge)
				{
					rangeEnd = foundElement - 1;
				}
				else
				{
					rangeStart = foundElement + 1;
				}
			}

			return lastFoundElement;
		}

		/// <summary>
		/// Осуществляет двоичный поиск города по сортированному индексу.
		/// Возвращает идентификатор первого найденного соответствия.
		/// </summary>
		/// <param name="city">Название города</param>
		/// <param name="rangeStart">
		/// Принимает на вход идентификатор первого элемента интервала, в котором нужно осуществлять поиск.
		/// Возвращает идентификатор первого элемента интервала, в котором найден город
		/// </param>
		/// <param name="rangeEnd">
		/// Принимает на вход идентификатор последнего элемента интервала, в котором нужно осуществлять поиск.
		/// Возвращает идентификатор последнего элемента интервала, в котором найден город
		/// </param>
		/// <returns>true, если город найден. Иначе false</returns>
		private unsafe int BinarySearchByCityName(sbyte* city, ref int rangeStart, ref int rangeEnd)
		{
			if (rangeStart > rangeEnd)
			{
				return -1;
			}

			int rangeMiddle = (rangeStart + rangeEnd) / 2;
			bool elementFound = false;
			while (true)
			{
				LocationRecord rec = GetLocationRecordByNameIndex(rangeMiddle);
				int comparisonResult = Utils.CompareSbpCity(rec.city, city, 24);
				if (comparisonResult == 0)
				{
					elementFound = true;
					break;
				}

				if (rangeStart == rangeEnd)
				{
					break;
				}

				if (comparisonResult == 1)
				{
					rangeEnd = rangeMiddle - 1;
				}
				else
				{
					rangeStart = rangeMiddle + 1;
				}

				if (rangeEnd < rangeStart)
				{
					break;
				}

				rangeMiddle = (rangeStart + rangeEnd) / 2;
			}

			return (elementFound ? rangeMiddle : -1);
		}

		/// <summary>
		/// Обеспечивает двоичный поиск по индексу интервалов IP-адресов
		/// </summary>
		private class IpRecordComparer : IComparer<IpRecord>
		{
			public static readonly IpRecordComparer Instance = new IpRecordComparer();

			private IpRecordComparer()
			{
			}

			public int Compare(IpRecord x, IpRecord y)
			{
				return x.ip_from.CompareTo(y.ip_from);
			}
		}
	}
}
