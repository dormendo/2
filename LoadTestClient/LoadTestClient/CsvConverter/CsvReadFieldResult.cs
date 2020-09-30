using System;
using System.Collections.Generic;
using System.Text;

namespace LoadTestClient
{
	/// <summary>
	/// Результат чтения поля
	/// </summary>
	public enum CsvReadFieldResult
	{
		/// <summary>
		/// Поле прочитано успешно
		/// </summary>
		Success = 0,

		/// <summary>
		/// Обнаружен символ ограничителя поля внутри поля без ограничителей
		/// </summary>
		QuoteInNqField = 1,

		/// <summary>
		/// Поле с ограничителями содержит символы после конечного ограничителя
		/// </summary>
		DelimiterExpected = 2,

		/// <summary>
		/// Не найден закрывающий ограничитель
		/// </summary>
		QuoteNotFound = 3
	}
}
