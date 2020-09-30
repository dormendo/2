using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tips24.PaymentService.SbReg.Csv
{
	/// <summary>
	/// Результат обработки следующей записи
	/// </summary>
	public class CsvReadRecordResult
	{
		/// <summary>
		/// Считанные поля
		/// </summary>
		public List<string> Fields;

		/// <summary>
		/// Результат чтения записи
		/// </summary>
		public CsvReadLineResult LineResult;

		/// <summary>
		/// Результат чтения поля. Используется для вывода сообщений об ошибках
		/// </summary>
		public CsvReadFieldResult FieldResult;

		/// <summary>
		/// Номер строки в файле, на которой произошла ошибка
		/// </summary>
		public int LineNumber;

		/// <summary>
		/// Номер позиции в строке, на которой произошла ошибка
		/// </summary>
		public int LinePosition;
	}
}
