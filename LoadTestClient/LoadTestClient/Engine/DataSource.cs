using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestClient
{
	class DataSource
	{
		private string _dataFilePath;

		private List<string> _columns = new List<string>();

		private Dictionary<string, int> _columnDictionary = new Dictionary<string, int>();

		private List<DataSourceRow> _data = new List<DataSourceRow>();

		private int _currentRowIndex = 0;

		private object _lock = new object();

		internal IReadOnlyList<string> Columns => _columns.AsReadOnly();

		internal int Count => _data.Count;

		internal DataSource(string dataFilePath)
		{
			_dataFilePath = dataFilePath;
		}

		internal void Load()
		{
			using (StreamReader sr = new StreamReader(_dataFilePath))
			{
				CsvLineReader reader = new CsvLineReader(sr);
				_columns = reader.ReadAndValidate();
				if (_columns.Count == 0)
				{
					throw new Exception("Файл данных из настройки конфигурации TestPlan.DataCsvFile должен содержать в первой строке названия колонок, разделённые символом \";\"");
				}

				SetColumnDictionary();

				int rowIndex = 0;
				while (!sr.EndOfStream)
				{
					List<string> data = reader.ReadAndValidate();
					if (data.Count > _columns.Count)
					{
						data.RemoveRange(_columns.Count, data.Count - _columns.Count);
					}
					else if (data.Count < _columns.Count)
					{
						for (int i = data.Count; i < _columns.Count; i++)
						{
							data.Add("");
						}
					}

					_data.Add(new DataSourceRow(_columns, _columnDictionary, data, rowIndex + 1));
					rowIndex++;
				}
			}
		}

		private void SetColumnDictionary()
		{
			for (int i = 0; i < _columns.Count; i++)
			{
				string column = _columns[i];
				if (_columnDictionary.ContainsKey(column))
				{
					throw new Exception("Файл данных из настройки конфигурации TestPlan.DataCsvFile содержит поля с неуникальными названиями");
				}

				_columnDictionary.Add(column, i);
			}
		}

		internal DataSourceRow GetNextRow()
		{
			int index = 0;
			lock (_lock)
			{
				_currentRowIndex++;
				if (_currentRowIndex == _data.Count)
				{
					_currentRowIndex = 0;
				}

				index = _currentRowIndex;
			}

			return _data[index];
		}

		internal DataSourceRow GetRow(int rowIndex)
		{
			return _data[rowIndex];
		}
	}
}
