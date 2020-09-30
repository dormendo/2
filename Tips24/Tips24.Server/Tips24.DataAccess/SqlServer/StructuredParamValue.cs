using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.SqlServer.Server;

namespace Tips24.DataAccess
{
	public class StructuredParamValue : IEnumerable<SqlDataRecord>
	{
		private readonly SqlMetaData[] _metadata;

		private readonly List<SqlDataRecord> _records;

		private int _fieldIndex = -1;

		private SqlDataRecord _record;

		public StructuredParamValue(params SqlMetaData[] metadata)
			: this(metadata, 10)
		{
		}

		public StructuredParamValue(SqlMetaData[] metadata, int initialCapacity)
		{
			this._metadata = metadata;
			this._records = new List<SqlDataRecord>(initialCapacity);
		}

		public void NewRecord()
		{
			this._record = new SqlDataRecord(this._metadata);
			this._records.Add(this._record);
			this._fieldIndex = 0;
		}

		public void AddDBNull()
		{
			this.ValidateIndex();
			this._record.SetDBNull(this._fieldIndex);
			this._fieldIndex++;
		}

		public void AddInt32(int value)
		{
			this.ValidateIndex();
			this._record.SetInt32(this._fieldIndex, value);
			this._fieldIndex++;
		}

		public void AddInt32(int? value)
		{
			if (value.HasValue)
			{
				this.AddInt32(value.Value);
			}
			else
			{
				this.AddDBNull();
			}
		}

		public void AddInt16(short value)
		{
			this.ValidateIndex();
			this._record.SetInt16(this._fieldIndex, value);
			this._fieldIndex++;
		}

		public void AddInt16(short? value)
		{
			if (value.HasValue)
			{
				this.AddInt16(value.Value);
			}
			else
			{
				this.AddDBNull();
			}
		}

		public void AddByte(byte value)
		{
			this.ValidateIndex();
			this._record.SetByte(this._fieldIndex, value);
			this._fieldIndex++;
		}

		public void AddByte(byte? value)
		{
			if (value.HasValue)
			{
				this.AddByte(value.Value);
			}
			else
			{
				this.AddDBNull();
			}
		}

		public void AddGuid(Guid value)
		{
			this.ValidateIndex();
			this._record.SetGuid(this._fieldIndex, value);
			this._fieldIndex++;
		}

		public void AddGuid(Guid? value)
		{
			if (value.HasValue)
			{
				this.AddGuid(value.Value);
			}
			else
			{
				this.AddDBNull();
			}
		}

		public void AddDecimal(decimal value)
		{
			this.ValidateIndex();
			this._record.SetDecimal(this._fieldIndex, value);
			this._fieldIndex++;
		}

		public void AddDecimal(decimal? value)
		{
			if (value.HasValue)
			{
				this.AddDecimal(value.Value);
			}
			else
			{
				this.AddDBNull();
			}
		}

		public void AddString(string value)
		{
			if (value != null)
			{
				this.ValidateIndex();
				this._record.SetString(this._fieldIndex, value);
				this._fieldIndex++;
			}
			else
			{
				this.AddDBNull();
			}
		}

		public void AddFloat(float value)
		{
			this.ValidateIndex();
			this._record.SetFloat(this._fieldIndex, value);
			this._fieldIndex++;
		}

		public void AddFloat(float? value)
		{
			if (value.HasValue)
			{
				this.AddFloat(value.Value);
			}
			else
			{
				this.AddDBNull();
			}
		}

		public void AddBoolean(bool value)
		{
			this.ValidateIndex();
			this._record.SetBoolean(this._fieldIndex, value);
			this._fieldIndex++;
		}

		public void AddBoolean(bool? value)
		{
			if (value.HasValue)
			{
				this.AddBoolean(value.Value);
			}
			else
			{
				this.AddDBNull();
			}
		}

		public void AddInt64(long value)
		{
			this.ValidateIndex();
			this._record.SetInt64(this._fieldIndex, value);
			this._fieldIndex++;
		}

		public void AddInt64(long? value)
		{
			if (value.HasValue)
			{
				this.AddInt64(value.Value);
			}
			else
			{
				this.AddDBNull();
			}
		}

		public void AddDateTime(DateTime value)
		{
			this.ValidateIndex();
			this._record.SetDateTime(this._fieldIndex, value);
			this._fieldIndex++;
		}

		public void AddDateTime(DateTime? value)
		{
			if (value.HasValue)
			{
				this.AddDateTime(value.Value);
			}
			else
			{
				this.AddDBNull();
			}
		}

		public void AddTimeSpan(TimeSpan value)
		{
			this.ValidateIndex();
			this._record.SetTimeSpan(this._fieldIndex, value);
			this._fieldIndex++;
		}

		public void AddTimeSpan(TimeSpan? value)
		{
			if (value.HasValue)
			{
				this.AddTimeSpan(value.Value);
			}
			else
			{
				this.AddDBNull();
			}
		}

		public IEnumerator<SqlDataRecord> GetEnumerator()
		{
			return this._records.GetEnumerator();
		}

		private void ValidateIndex()
		{
			if (this._fieldIndex < 0 || this._fieldIndex >= this._metadata.Length || this._record == null)
			{
				throw new InvalidOperationException();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._records.GetEnumerator();
		}

		public SqlDataRecord this[int index]
		{
			get
			{
				return this._records[index];
			}
		}

		public int Count
		{
			get
			{
				return this._records.Count;
			}
		}

		public void Clear()
		{
			this._records.Clear();
		}
	}
}
