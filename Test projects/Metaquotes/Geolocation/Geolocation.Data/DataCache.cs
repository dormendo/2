using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Geolocation.Data
{
	/// <summary>
	/// Кеш данных
	/// </summary>
	internal class DataCache
	{
		/// <summary>
		/// Флаг доступности базы данных для обработки запросов
		/// </summary>
		private volatile bool _isDataAvailable = false;

		/// <summary>
		/// Флаг полного завершения создания кеша данных
		/// </summary>
		private volatile bool _isCacheAvailable = false;

		/// <summary>
		/// Флаг режима доступности.
		/// true, если БД обрабатывает запросы сразу после загрузки структур данных в память.
		/// false, если БД доступна только после полного создания кеша.
		/// </summary>
		private bool _availableOnDbLoad = false;

		/// <summary>
		/// Флаг завершённости процедуры по созданию кеша расположений
		/// </summary>
		private volatile bool _locationCacheIsLoaded = false;

		/// <summary>
		/// Уровень параллелизма при генерации кеша
		/// </summary>
		private int _cacheLoadOperationsDop;

		/// <summary>
		/// Ссылка на БД
		/// </summary>
		private Database _db;

		/// <summary>
		/// Кеш результатов корректных запросов по названию города (JSON/UTF-8)
		/// Поскольку нет информации о количестве городов, используется хеш-таблица, хранящая буферы результатов
		/// </summary>
		private ConcurrentDictionary<string, byte[]> _cityCache;

		/// <summary>
		/// Кеш результатов запросов по интервалу IP (JSON/UTF-8)
		/// Поскольку количество местоположений известно, используется массив буферов
		/// </summary>
		private byte[][] _locationCache;

		internal DataCache(string fileName, bool availableOnFileLoad)
		{
			_db = new Database(fileName);
			_availableOnDbLoad = availableOnFileLoad;

			// Предполагается, что приложение запущено на машине с несколькими ядрами
			// Если кеш собирается в процессе инициализации, используем все доступные ядра.
			// Если кеш собирается после инициализации, 25% ядер, но не менее одного.
			_cacheLoadOperationsDop = (_availableOnDbLoad ? Environment.ProcessorCount / 4 : Environment.ProcessorCount);
			if (_cacheLoadOperationsDop == 0)
			{
				_cacheLoadOperationsDop = 1;
			}
		}

		/// <summary>
		/// Инициализирует кеш данных
		/// </summary>
		internal void Initialize()
		{
			_db.Initialize();

			_locationCache = new byte[_db.RecordCount][];
			_cityCache = new ConcurrentDictionary<string, byte[]>();

			if (_availableOnDbLoad)
			{
				// Может обрабатывать данные до полного сбора кеша
				this._isDataAvailable = true;
				// Запускаем сбор кеша после завершения инициализации в режиме щадящей нагрузки на ресурсы процессора
				Task.Run(() => AcquireCaches());
			}
			else
			{
				// Максимально производительный сбор кеша
				AcquireCaches();
			}
		}

		/// <summary>
		/// Собирает структуры с кешем данных
		/// </summary>
		private void AcquireCaches()
		{
			AcquireLocationCache();
			AcquireCityCache();

			_isCacheAvailable = true;
			_isDataAvailable = true;
		}

		/// <summary>
		/// Возвращает информацию о местоположении по IP-адресу
		/// </summary>
		/// <param name="ip">IP-адрес</param>
		/// <returns>Возвращает ответ, готовый к отправке по сети</returns>
		internal unsafe byte[] GetLocationByIp(uint ip)
		{
			if (!_isDataAvailable)
			{
				return null;
			}

			byte[] result = null;
			uint locationIndex;
			if (_db.GetLocationIndexByIp(ip, out locationIndex))
			{
				if (!_isCacheAvailable)
				{
					Thread.MemoryBarrier();
					result = _locationCache[(int)locationIndex];
					if (result == null)
					{
						JsonSerializer.Buffer buffer = JsonSerializer.GetBufferForLocation();
						result = GetLocationJson((int)locationIndex, buffer);
						_locationCache[locationIndex] = result;
					}
				}
				else
				{
					return _locationCache[locationIndex];
				}
			}

			return result;
		}

		/// <summary>
		/// Возвращает информацию о местоположениях, соответствующих городу
		/// </summary>
		/// <param name="city">Название города</param>
		/// <returns>Возвращает ответ, готовый к отправке по сети</returns>
		internal byte[] GetLocationsByCity(string city)
		{
			if (!_isDataAvailable)
			{
				return null;
			}

			byte[] bytes = null;
			if (!_cityCache.TryGetValue(city, out bytes) && !_isCacheAvailable)
			{
				int firstElement, lastElement;
				if (!_db.GetLocationsByCityName(city, out firstElement, out lastElement))
				{
					return null;
				}

				bytes = AcquireCityResult(city, firstElement, lastElement);
			}

			return bytes;
		}


		/// <summary>
		/// Собирает кеш для города и возвращает его значение
		/// </summary>
		/// <param name="city">Название города</param>
		/// <param name="firstElement">Первый элемент в индексе по названию города</param>
		/// <param name="lastElement">Последний элемент в индексе по названию города</param>
		/// <returns>Возвращает ответ, готовый к отправке по сети</returns>
		private byte[] AcquireCityResult(string city, int firstElement, int lastElement)
		{
			if (!_locationCacheIsLoaded)
			{
				JsonSerializer.Buffer buffer = null;
				for (int i = firstElement; i <= lastElement; i++)
				{
					byte[] result;
					int locationIndex = _db.GetLocationIdByNameIndex(i);

					Thread.MemoryBarrier();
					if (_locationCache[locationIndex] == null)
					{
						if (buffer == null)
						{
							buffer = JsonSerializer.GetBufferForLocation();
						}

						result = GetLocationJson(locationIndex, buffer);
						_locationCache[locationIndex] = result;
					}
				}
			}

			return AddCityCacheEntry(city, firstElement, lastElement);
		}

		/// <summary>
		/// Служебный метод
		/// </summary>
		internal unsafe string[] GetAllCities()
		{
			if (_isCacheAvailable)
			{
				return new List<string>(_cityCache.Keys).ToArray();
			}
			else
			{
				HashSet<string> set = new HashSet<string>(StringComparer.Ordinal);
				for (int i = 0; i < _db.RecordCount; i++)
				{
					LocationRecord rec = _db.GetLocationRecordById(i);
					string city = new string(rec.city);
					if (!set.Contains(city))
					{
						set.Add(city);
					}
				}

				return set.ToArray();
			}
		}

		/// <summary>
		/// Собирает кеш местоположений
		/// </summary>
		private void AcquireLocationCache()
		{
			Parallel.For<JsonSerializer.Buffer>(0, _db.RecordCount,
				new ParallelOptions() { MaxDegreeOfParallelism = _cacheLoadOperationsDop },
				() => JsonSerializer.GetBufferForLocation(),
				(i, state, buffer) =>
				{
					Thread.MemoryBarrier();
					if (_locationCache[i] == null)
					{
						_locationCache[i] = GetLocationJson(i, buffer);
					}

					return buffer;
				},
				(buffer) => { });

			_locationCacheIsLoaded = true;
		}

		/// <summary>
		/// Формирует значение кеша местоположения
		/// </summary>
		/// <param name="i">Идентификатор местоположения</param>
		/// <param name="buffer">Буфер</param>
		/// <returns></returns>
		private unsafe byte[] GetLocationJson(int i, JsonSerializer.Buffer buffer)
		{
			buffer.Reset();
			LocationRecord rec = _db.GetLocationRecordById(i);
			JsonSerializer.WriteLocationRecord(buffer, rec);
			return buffer.GetByteArray();
		}

		/// <summary>
		/// Информация об интервалах индекса по названиям городов
		/// </summary>
		private struct IndexCacheItem
		{
			/// <summary>
			/// Начало интервала
			/// </summary>
			public int StartIndex;

			/// <summary>
			/// Конец интервала
			/// </summary>
			public int EndIndex;

			/// <summary>
			/// Конструктор
			/// </summary>
			/// <param name="startIndex">Начало интервала</param>
			/// <param name="endIndex">Конец интервала</param>
			public IndexCacheItem(int startIndex, int endIndex)
			{
				this.StartIndex = startIndex;
				this.EndIndex = endIndex;
			}
		}

		/// <summary>
		/// Собирает кеш городов
		/// </summary>
		private unsafe void AcquireCityCache()
		{
			// Сначала проходим по индексу городов и собираем интервалы, соответствующие одному городу
			List<IndexCacheItem> indexCache = new List<IndexCacheItem>(_db.RecordCount / 10);
			byte[] baCurrentCity = new byte[24];
			fixed (byte* bpCurrentCity = &baCurrentCity[0])
			{
				sbyte* currentCity = (sbyte*)bpCurrentCity;
				int startIndex = 0;
				for (int i = 0; i < _db.RecordCount; i++)
				{
					LocationRecord rec = _db.GetLocationRecordByNameIndex(i);
					if (i == 0 || Utils.CompareSbpCity(currentCity, rec.city, 24) != 0)
					{
						if (i > 0)
						{
							indexCache.Add(new IndexCacheItem(startIndex, i - 1));
							startIndex = i;
						}
						Utils.CopyMemory(currentCity, rec.city, 24);
					}
				}

				indexCache.Add(new IndexCacheItem(startIndex, _db.RecordCount - 1));
			}

			// Затем в параллельном режиме формируем записи в кеше для городов
			int indexCacheLength = indexCache.Count;
			int currentItemIndex = -1;
			Task[] tasks = new Task[_cacheLoadOperationsDop];
			for (int i = 0; i < _cacheLoadOperationsDop; i++)
			{
				Task task = Task.Run(() =>
				{
					while (true)
					{
						int cacheIndex = Interlocked.Increment(ref currentItemIndex);
						if (cacheIndex >= indexCacheLength)
						{
							return;
						}

						IndexCacheItem cacheItem = indexCache[cacheIndex];
						LocationRecord rec = _db.GetLocationRecordByNameIndex(cacheItem.StartIndex);
						AddCityCacheEntry(new string(rec.city), cacheItem.StartIndex, cacheItem.EndIndex);
					}
				});
				tasks[i] = task;
			}

			Task.WaitAll(tasks);
		}

		/// <summary>
		/// Формирует значение кеша для города
		/// </summary>
		/// <param name="city">Название города</param>
		/// <param name="startIndex">Начало интервала</param>
		/// <param name="endIndex">Конец интервала</param>
		/// <returns></returns>
		private byte[] AddCityCacheEntry(string city, int startIndex, int endIndex)
		{
			byte[] result;
			if (_cityCache.TryGetValue(city, out result))
			{
				return result;
			}

			int bufferSize = 1;
			for (int i = startIndex; i <= endIndex; i++)
			{
				bufferSize += _locationCache[_db.GetLocationIdByNameIndex(i)].Length + 1;
			}

			JsonSerializer.Buffer buffer = new JsonSerializer.Buffer(bufferSize);

			JsonSerializer.StartArray(buffer);
			for (int i = startIndex; i <= endIndex; i++)
			{
				if (i > startIndex)
				{
					JsonSerializer.Comma(buffer);
				}

				buffer.Write(_locationCache[_db.GetLocationIdByNameIndex(i)]);
			}

			JsonSerializer.EndArray(buffer);

			result = buffer.GetByteArray();
			_cityCache.TryAdd(city, result);
			return result;
		}
	}
}
