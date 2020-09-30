using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;

namespace LoadTestClient
{
	/// <summary>
	/// Обеспечивает получение записей CSV из потока строковых данных
	/// </summary>
	public class CsvLineReader
	{
		#region Поля

		private char _quote;

		private char _delimiter;

		private TextReader _reader;

		private string _fileLine;

		private int _lineNumber = 0;

		private int _errorPosition;

		private int _position;

		private StringBuilder _builder = new StringBuilder();

		#endregion

		#region Свойства

		private bool NeedNewLine
		{
			get
			{
				return this._fileLine == null || this._fileLine.Length <= this._position;
			}
		}

		#endregion

		#region Конструкторы

		/// <summary>
		/// Конструктор и инциализация
		/// </summary>
		public CsvLineReader(TextReader reader, char quote = '\"', char delimiter = ';')
		{
			this._reader = reader;
			this._quote = quote;
			this._delimiter = delimiter;
		}

		#endregion Конструкторы

		#region Методы

		/// <summary>
		/// Производит разбор одной записи
		/// </summary>
		/// <param name="record">Запись</param>
		/// <returns></returns>
		public static List<string> ParseRecord(string record)
		{
			using (StringReader sr = new StringReader(record))
			{
				CsvLineReader reader = new CsvLineReader(sr);
				return reader.ReadAndValidate();
			}
		}

		/// <summary>
		/// Считывает новую строку
		/// </summary>
		/// <returns>Результат</returns>
		public CsvReadRecordResult Read()
		{
			do
			{
				if (!this.GetNewLineFromFile())
				{
					return new CsvReadRecordResult { LineResult = CsvReadLineResult.Eof };
				}
			}
			while (string.IsNullOrEmpty(this._fileLine));

			List<string> fields = null;
			while (true)
			{
				this._builder.Clear();

				CsvReadFieldResult result = this.ReadNewField();
				if (result == CsvReadFieldResult.Success)
				{
					if (fields == null)
					{
						fields = new List<string>();
					}

					fields.Add(this._builder.ToString());
					if (this.NeedNewLine)
					{
						return new CsvReadRecordResult { LineResult = CsvReadLineResult.Success, Fields = fields };
					}
				}
				//else if (result == ReadFieldResult.DelimiterExpected)
				//{
				//	return new ReadRecordResult { FieldResult = result, LineNumber = this._lineNumber, LinePosition = this._position, LineResult = ReadLineResult.Error };
				//	int delimiterPosition = this._fileLine.IndexOf(';', this._position + 1);
				//	this._position = (delimiterPosition > 0 ? this._position = delimiterPosition + 1 : this._fileLine.Length);
				//}
				else
				{
					return new CsvReadRecordResult { FieldResult = result, LineNumber = this._lineNumber, LinePosition = this._errorPosition, LineResult = CsvReadLineResult.Error };
				}
			}
		}

		/// <summary>
		/// Считывает новую строку и вбрасывает исключение в случае ошибки разбора
		/// </summary>
		/// <returns></returns>
		public List<string> ReadAndValidate()
		{
			CsvReadRecordResult result = this.Read();
			if (result.LineResult == CsvReadLineResult.Success)
			{
				return result.Fields;
			}
			else if (result.LineResult == CsvReadLineResult.Eof)
			{
				return null;
			}
			else if (result.FieldResult == CsvReadFieldResult.DelimiterExpected)
			{
				throw new Exception($"Ошибка разбора файла с данными, строка: {result.LineNumber}, позиция: {result.LinePosition}");
			}
			else if (result.FieldResult == CsvReadFieldResult.QuoteInNqField)
			{
				throw new Exception($"Ошибка разбора файла с данными, строка: {result.LineNumber}, позиция: {result.LinePosition}");
			}
			else if (result.FieldResult == CsvReadFieldResult.QuoteNotFound)
			{
				throw new Exception($"Ошибка разбора файла с данными, строка: {result.LineNumber}, позиция: {result.LinePosition}");
			}
			else
			{
				return null;
			}
		}

		private CsvReadFieldResult ReadNewField()
		{
			char firstChar = this._fileLine[this._position];
			if (firstChar == this._quote)
			{
				return this.ReadQuotedField();
			}
			else if (firstChar == this._delimiter)
			{
				this._position++;
				return CsvReadFieldResult.Success;
			}
			else
			{
				return this.ReadNonQuotedString();
			}
		}

		private CsvReadFieldResult ReadQuotedField()
		{
			this._position++;
			CsvReadFieldResult result = InnerReadQuotedField();
			if (result == CsvReadFieldResult.Success && !this.NeedNewLine)
			{
				if (this._fileLine[this._position] != this._delimiter)
				{
					this._errorPosition = this._position + 1;
					return CsvReadFieldResult.DelimiterExpected;
				}
				else
				{
					this._position++;
				}
			}

			return result;
		}

		private CsvReadFieldResult InnerReadQuotedField()
		{
			while (true)
			{
				int fieldStartPosition = this._position;
				int quotePosition = this._fileLine.IndexOf(this._quote, fieldStartPosition);
				if (quotePosition >= fieldStartPosition)
				{
					if (quotePosition > fieldStartPosition)
					{
						this._builder.Append(this._fileLine, fieldStartPosition, quotePosition - fieldStartPosition);
					}

					this._position = quotePosition + 1;

					if (this.NeedNewLine)
					{
						return CsvReadFieldResult.Success;
					}

					if (this._fileLine[this._position] == this._quote)
					{
						this._builder.Append(this._quote);
						this._position++;
					}
					else
					{
						return CsvReadFieldResult.Success;
					}
				}
				else if (quotePosition < 0)
				{
					if (this._fileLine.Length > fieldStartPosition)
					{
						this._builder.Append(this._fileLine, fieldStartPosition, this._fileLine.Length - fieldStartPosition);
					}

					int fileLine = this._fileLine.Length;
					if (!this.GetNewLineFromFile())
					{
						this._errorPosition = fileLine;
						return CsvReadFieldResult.QuoteNotFound;
					}
					else
					{
						this._builder.AppendLine();
					}
				}
			}
		}

		private CsvReadFieldResult ReadNonQuotedString()
		{
			int fieldStartPosition = this._position;
			int delimiterPosition = this._fileLine.IndexOf(this._delimiter, fieldStartPosition);

			if (delimiterPosition < 0 && this._fileLine.Length == fieldStartPosition)
			{
				return CsvReadFieldResult.Success;
			}

			int charsToCopy = (delimiterPosition > fieldStartPosition ? delimiterPosition - fieldStartPosition : this._fileLine.Length - fieldStartPosition);
			int quotePosition = this._fileLine.IndexOf(this._quote, fieldStartPosition, charsToCopy);
			if (quotePosition >= 0)
			{
				this._errorPosition = quotePosition + 1;
				return CsvReadFieldResult.QuoteInNqField;
			}

			this._builder.Append(this._fileLine, fieldStartPosition, charsToCopy);
			this._position = (delimiterPosition < 0 ? this._fileLine.Length : delimiterPosition + 1);
			return CsvReadFieldResult.Success;
		}

		private bool GetNewLineFromFile()
		{
			this._fileLine = this._reader.ReadLine();

			if (this._fileLine == null)
			{
				return false;
			}
			else
			{
				this._position = 0;
				this._lineNumber++;
				return true;
			}
		}

		#endregion
	}
}
