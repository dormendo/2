using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestClient
{
	class FunctionPart : IContentPart
	{
		internal enum Type
		{
			NewGuid,
			StartToday,
			StartDateTime,
			Today,
			DateTime,
			ThreadNumber,
			DataRowNumber
		}

		Type _type;
		byte[] _cachedValue;

		internal FunctionPart(Type type)
		{
			_type = type;
		}

		async Task IContentPart.Write(Stream stream, WorkItemData workItem)
		{
			if (_type == Type.StartToday || _type == Type.StartDateTime)
			{
				if (_cachedValue == null)
				{
					_cachedValue = Encoding.UTF8.GetBytes(workItem.StartTime.ToString(_type == Type.StartToday ? "yyyy-MM-dd" : "O"));
				}

				await stream.WriteAsync(_cachedValue, 0, _cachedValue.Length);
				return;
			}

			string value = null;
			if (_type == Type.NewGuid)
			{
				value = Guid.NewGuid().ToString();
			}
			else if (_type == Type.Today)
			{
				value = DateTime.Today.ToString("yyyy-MM-dd");
			}
			else if (_type == Type.Today)
			{
				value = DateTime.Now.ToString("O");
			}
			else if (_type == Type.ThreadNumber)
			{
				value = workItem.ThreadNumber.ToString();
			}
			else if (_type == Type.DataRowNumber)
			{
				value = workItem.Row.RowNumber.ToString();
			}

			if (!string.IsNullOrEmpty(value))
			{
				byte[] ba = Encoding.UTF8.GetBytes(value);
				await stream.WriteAsync(ba, 0, ba.Length);
			}
		}
	}
}
