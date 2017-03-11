namespace Geolocation.Data
{
	/// <summary>
	/// Интерфейс загрузчика данных из БД во внутренние структуры механизма обработки запросов
	/// </summary>
	internal interface IDbLoader
	{
		/// <summary>
		/// Инициализирует структуры данных информацией из файла БД
		/// </summary>
		/// <param name="fileName">Путь к файлу БД</param>
		/// <param name="holder">Хранилище структур данных БД</param>
		void Load(string fileName, DataHolder holder);
	}
}
