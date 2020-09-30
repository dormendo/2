using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestClient
{
	class DataSourceRow
	{
		private List<string> _columns;

		private Dictionary<string, int> _columnsDictionary;

		private List<string> _values;

		internal int RowNumber { get; private set; }

		internal DataSourceRow(List<string> columns, Dictionary<string, int> columnsDictionary, List<string> values, int dataRowIndex)
		{
			_columns = columns;
			_columnsDictionary = columnsDictionary;
			_values = values;
			RowNumber = dataRowIndex;
		}

		internal string GetValue(int columnIndex)
		{
			return _values[columnIndex];
		}

		internal string GetValue(string columnName)
		{
			int columnIndex = GetOrdinal(columnName);
			return GetValue(columnIndex);
		}

		internal int GetOrdinal(string columnName)
		{
			return _columnsDictionary[columnName];
		}
	}
}
