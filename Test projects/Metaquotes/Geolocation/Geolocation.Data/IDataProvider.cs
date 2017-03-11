namespace Geolocation.Data
{
	/// <summary>
	/// Интерфейс открытого доступа к БД.
	/// Методы возвращают готовые к отправке по сети сообщения, сериализованные в JSON в кодировке UTF-8
	/// </summary>
	public interface IDataProvider
	{
		/// <summary>
		/// Возвращает информацию о расположении по IP-адресу
		/// </summary>
		/// <param name="ip">IPv4 в строковом выражении</param>
		/// <returns>Сообщение, готовое к отправке по сети, либо null, если владелец IP не найден</returns>
		byte[] GetLocationByIp(string ip);

		/// <summary>
		/// Возвращает информацию о расположениях по названию города с учётом регистра
		/// </summary>
		/// <param name="city">Название города</param>
		/// <returns>Сообщение, готовое к отправке по сети, либо null, если город не найден</returns>
		byte[] GetLocationsByCity(string city);
	}
}