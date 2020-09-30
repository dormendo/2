using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Tips24.PayoutService.PayU
{
	/*
	merchantCode / Код продавца. Это код можно посмотреть на странице https://secure.payu.ru/cpanel/account_settings.php. Обычно длинной 8 символов / codecode
	currency / Валюта / RUB
	amount / Сумма транзакции / 45.00
	outerId / Идентификатор сделки в системе магазина (МФО). Можно передавать любой набор цифр / 12365126
	senderFirstName / Имя отправителя (можно передавать название юр лица) / Ivan
	senderLastName / Фамилия отправителя (можно передавать название юр лица) / Ivanov
	senderEmail / Адрес электронной почты отправителя / senderEmail@mail.com
	senderPhone / Телефон отправителя (в числовом формате) / 9998887766
	clientEmail / Адрес электронной почты клиента / clientEmail@mail.com
	clientFirstName / Имя клиента (если не известно, то можно передать Уважаемый Клиент) / Ivan
	clientLastName / Фамилия клиента / Ivanov
	clientCity / Город клиента / -
	clientAddress / Адрес клиента / -
	clientPostalCode / Индекс клиента / 123123
	clientCountryCode / Код страны клиента / RU
	ccnumber / Номер кредитной карты / 4111111111111111
	desc / Описание / Произвольный текст
	timestamp / UNIX timestamp / 1480062984
	signature / Подпись запроса (формируется автоматически на стороне магазина) / adfbadb6072d4348dcbbcc9ee8f63623
	 */
	public class PayuPayoutRequest
	{
		private const string amount = "amount";
		private const string ccnumber = "ccnumber";
		private const string clientAddress = "clientAddress";
		private const string clientCity = "clientCity";
		private const string clientCountryCode = "clientCountryCode";
		private const string clientEmail = "clientEmail";
		private const string clientFirstName = "clientFirstName";
		private const string clientLastName = "clientLastName";
		private const string clientPostalCode = "clientPostalCode";
		private const string currency = "currency";
		private const string desc = "desc";
		private const string merchantCode = "merchantCode";
		private const string outerId = "outerId";
		private const string senderEmail = "senderEmail";
		private const string senderFirstName = "senderFirstName";
		private const string senderLastName = "senderLastName";
		private const string senderPhone = "senderPhone";
		private const string timestamp = "timestamp";

		private SortedDictionary<string, string> _dict = new SortedDictionary<string, string>();

		private string signature;

		public PayuPayoutRequest(PayoutRequest request, string merchCode, string secretKey)
		{
			this._dict.Add(amount, request.PayoutAmount.ToString(CultureInfo.InvariantCulture));
			this._dict.Add(ccnumber, request.EmployeeData.CardNumber);
			//this._dict.Add(clientAddress, );
			//this._dict.Add(clientCity, );
			this._dict.Add(clientCountryCode, "RU");
			this._dict.Add(clientEmail, request.EmployeeData.Email);
			this._dict.Add(clientFirstName, request.EmployeeData.FirstName);
			this._dict.Add(clientLastName, request.EmployeeData.LastName);
			//this._dict.Add(clientPostalCode, );
			this._dict.Add(currency, "RUB");
			this._dict.Add(desc, "Выплата подаренных чаевых в соответствии договором-офертой tips24.ru/3. Выплата произведена на основании агентского отчёта " + request.Id + " от " + request.CreateDateTime.ToString("dd.MM.yyyy") );
			this._dict.Add(merchantCode, merchCode);
			this._dict.Add(outerId, request.Id.ToString());
			this._dict.Add(senderEmail, "4epexa@gmail.com");
			this._dict.Add(senderFirstName, "ООО \"Чаевые-24\"");
			this._dict.Add(senderLastName, "ООО \"Чаевые-24\"");
			this._dict.Add(senderPhone, "9272441678");
			this._dict.Add(timestamp, DateTimeOffset.Now.ToUnixTimeSeconds().ToString());
		}
	}
}
