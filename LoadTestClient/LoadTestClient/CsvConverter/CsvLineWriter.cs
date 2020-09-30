using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LoadTestClient
{
	/// <summary>
	/// Сохраняет коллекцию полей в файле формата CSV
	/// </summary>
	public class CsvLineWriter
	{
		#region Поля

		/// <summary>
		/// Символ кавычки
		/// </summary>
		private readonly char QuoteChar = '\"';

		/// <summary>
		/// Символ разделителя
		/// </summary>
		private readonly char DelimiterChar = ';';

		/// <summary>
		/// Строка кавычки
		/// </summary>
		public string QuoteCharString;

		/// <summary>
		/// Строка двойной кавычки
		/// </summary>
		public string QuotedQuoteCharString;

		#endregion

		#region Конструкторы

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="quote">Символ кавычки</param>
		/// <param name="delimiter">Символ разделителя</param>
		public CsvLineWriter(char quote = '\"', char delimiter = ';')
		{
			this.QuoteChar = quote;
			this.DelimiterChar = delimiter;
			this.QuoteCharString = QuoteChar.ToString();
			this.QuotedQuoteCharString = this.QuoteCharString + this.QuoteCharString;
		}

		#endregion

		/// <summary>
		/// Записывает одну запись в результирующий файл.
		/// </summary>
		/// <param name="strings">Список строк</param>
		/// <param name="writer">Результирующий файл</param>
		public void WriteCsvRecord(IList<string> strings, TextWriter writer)
		{
			for (int i = 0; i < strings.Count; i++)
			{
				writer.Write(QuoteChar);
				string val = strings[i].Replace(QuoteCharString, QuotedQuoteCharString);
				writer.Write(val);
				writer.Write(QuoteChar);
				if (i < strings.Count - 1)
				{
					writer.Write(DelimiterChar);
				}
			}
			writer.WriteLine();
		}

		/// <summary>
		/// Формирует CSV-строку из набора значений
		/// </summary>
		/// <param name="strings">Значения</param>
		/// <returns>CSV-строка</returns>
		public string GetRecord(List<string> strings)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < strings.Count; i++)
			{
				if (i > 0)
				{
					sb.Append(DelimiterChar);
				}
				sb.Append(QuoteCharString).Append(strings[i].Replace(QuoteCharString, QuotedQuoteCharString)).Append(QuoteCharString);
			}

			return sb.ToString();
		}
	}
}
