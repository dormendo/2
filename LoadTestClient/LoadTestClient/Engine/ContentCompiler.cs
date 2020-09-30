using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestClient
{
	class ContentCompiler
	{
		private class PatternData
		{
			public FunctionPart.Type? FunctionType;

			public int ColumnOrdinal;
		}

		private string _template;

		private IReadOnlyList<string> _columns;

		private List<IContentPart> _parts = new List<IContentPart>();

		private Dictionary<string, PatternData> _patterns = new Dictionary<string, PatternData>();

		internal ContentCompiler(string template, IReadOnlyList<string> columns)
		{
			_template = template;
			_columns = columns;
		}

		internal void Compile()
		{
			_patterns.Add("{function:NewGuid}", new PatternData { FunctionType = FunctionPart.Type.NewGuid });
			_patterns.Add("{function:StartToday}", new PatternData { FunctionType = FunctionPart.Type.StartToday });
			_patterns.Add("{function:StartDateTime}", new PatternData { FunctionType = FunctionPart.Type.StartDateTime });
			_patterns.Add("{function:Today}", new PatternData { FunctionType = FunctionPart.Type.Today });
			_patterns.Add("{function:DateTime}", new PatternData { FunctionType = FunctionPart.Type.DateTime });
			_patterns.Add("{function:ThreadNumber}", new PatternData { FunctionType = FunctionPart.Type.ThreadNumber });
			_patterns.Add("{function:DataRowNumber}", new PatternData { FunctionType = FunctionPart.Type.DataRowNumber });

			if (_columns != null)
			{
				for (int i = 0; i < _columns.Count; i++)
				{
					string column = _columns[i];
					_patterns.Add("{data:" + column + "}", new PatternData { ColumnOrdinal = i });
				}
			}

			int minPatternLen = _patterns.Min(p => p.Key.Length);

			int contentStartIndex = 0;
			int indexOfStartIndex = 0;
			while (true)
			{
				int braceIndex = _template.IndexOf('{', indexOfStartIndex);
				if (braceIndex < 0 || _template.Length - braceIndex < minPatternLen)
				{
					break;
				}

				string foundPattern = null;
				foreach (string pattern in _patterns.Keys)
				{
					if (string.CompareOrdinal(pattern, 0, _template, braceIndex, pattern.Length) == 0)
					{
						foundPattern = pattern;
						break;
					}
				}

				if (foundPattern != null)
				{
					AddContentPart(contentStartIndex, braceIndex - contentStartIndex);
					AddPatternPart(foundPattern);
					contentStartIndex = braceIndex + foundPattern.Length;
					indexOfStartIndex = contentStartIndex;
				}
			}

			AddContentPart(contentStartIndex, _template.Length - contentStartIndex);
		}

		private void AddContentPart(int startIndex, int length)
		{
			if (length <= 0)
			{
				return;
			}

			ContentPart part = new ContentPart(_template.Substring(startIndex, length));
			_parts.Add(part);
		}

		private void AddPatternPart(string pattern)
		{
			PatternData data = _patterns[pattern];

			IContentPart part;
			if (data.FunctionType.HasValue)
			{
				part = new FunctionPart(data.FunctionType.Value);
			}
			else
			{
				part = new DataPart(data.ColumnOrdinal);
			}

			_parts.Add(part);
		}

		internal void WriteContent(Stream stream, WorkItemData wi)
		{
			for (int i = 0; i < _parts.Count; i++)
			{
				_parts[i].Write(stream, wi);
			}
		}
	}
}
