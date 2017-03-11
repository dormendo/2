namespace Geolocation.Data
{
	/// <summary>
	/// Реализация открытого интерфейса доступа к БД геолокации
	/// </summary>
	public class DataProvider : IDataProvider
	{
		/// <summary>
		/// Кеш данных
		/// </summary>
		DataCache _cache;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="fileName">Путь к файлу БД</param>
		/// <param name="fastAvailability">Режим инициализации БД. true, если БД должна быть доступна для запросов как можно быстрее. false, если требуется полная подготовка кеша</param>
		public DataProvider(string fileName, bool fastAvailability)
		{
			_cache = new DataCache(fileName, fastAvailability);
		}

		/// <summary>
		/// Инициалиазирует БД
		/// </summary>
		public void Initialize()
		{
			_cache.Initialize();
		}

		/// <summary>
		/// Возвращает информацию о расположении по IP-адресу
		/// </summary>
		/// <param name="ip">IPv4 в строковом выражении</param>
		/// <returns>Сообщение, готовое к отправке по сети, либо null, если владелец IP не найден</returns>
		public unsafe byte[] GetLocationByIp(string ip)
		{
			if (string.IsNullOrEmpty(ip))
			{
				return null;
			}

			uint uintIp;
			if (Utils.TryConvertIpToUint(ip.Trim(), out uintIp))
			{
				return _cache.GetLocationByIp(uintIp);
			}

			return null;
		}

		/// <summary>
		/// Возвращает информацию о расположениях по названию города с учётом регистра
		/// </summary>
		/// <param name="city">Название города</param>
		/// <returns>Сообщение, готовое к отправке по сети, либо null, если город не найден</returns>
		public byte[] GetLocationsByCity(string city)
		{
			if (string.IsNullOrEmpty(city))
			{
				return null;
			}

			return _cache.GetLocationsByCity(city);
		}

		/// <summary>
		/// Служебный метод
		/// </summary>
		public string[] GetAllCities()
		{
			return _cache.GetAllCities();
		}
	}
}
