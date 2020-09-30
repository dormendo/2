using System;
using System.Collections.Generic;
using System.Text;

namespace Tips24.PaymentService.SbReg.Csv
{
	/// <summary>
	/// Результат чтения записи
	/// </summary>
	public enum CsvReadLineResult
	{
		/// <summary>
		/// Строка прочитана успешно
		/// </summary>
		Success = 0,

		/// <summary>
		/// Ошибка при чтении строки
		/// </summary>
		Error = 1,

		/// <summary>
		/// Конец файла
		/// </summary>
		Eof = 2
	}
}
