using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Tips24
{
	public static class SqlDataReaderExtensions
	{
		public static async Task<T> GetValueAsync<T>(this SqlDataReader reader, string column)
		{
			CheckType<T>(reader, column);
			object value = await reader.GetFieldValueAsync<object>(reader.GetOrdinal(column));
			value = value is DBNull ? null : value;
			return (T)value;
		}

		public static T GetValue<T>(this SqlDataReader reader, string column)
		{
			CheckType<T>(reader, column);
			object value = reader.GetValue(reader.GetOrdinal(column));
			return value is DBNull ? default(T) : (T)value;
		}

		public static bool IsDBNull(this SqlDataReader reader, string columnName)
		{
			return reader.IsDBNull(reader.GetOrdinal(columnName));
		}

		#region GetXXXX("COLUMN_NAME");

		public static int GetInt32(this SqlDataReader reader, string column)
		{
			CheckType<int>(reader, column);
			return reader.GetInt32(reader.GetOrdinal(column));
		}

		public static short GetInt16(this SqlDataReader reader, string column)
		{
			CheckType<short>(reader, column);
			return reader.GetInt16(reader.GetOrdinal(column));
		}

		public static byte GetByte(this SqlDataReader reader, string column)
		{
			CheckType<byte>(reader, column);
			return reader.GetByte(reader.GetOrdinal(column));
		}

		public static byte[] GetBytes(this SqlDataReader reader, int columnOrdinal)
		{
			CheckType<byte[]>(reader, columnOrdinal);
			return reader.GetSqlBinary(columnOrdinal).Value;
		}

		public static byte[] GetBytes(this SqlDataReader reader, string column)
		{
			return reader.GetBytes(reader.GetOrdinal(column));
		}

		public static long GetInt64(this SqlDataReader reader, string column)
		{
			CheckType<long>(reader, column);
			return reader.GetInt64(reader.GetOrdinal(column));
		}

		public static decimal GetDecimal(this SqlDataReader reader, string column)
		{
			CheckType<decimal>(reader, column);
			return reader.GetDecimal(reader.GetOrdinal(column));
		}

		public static float GetFloat(this SqlDataReader reader, string column)
		{
			CheckType<float>(reader, column);
			return reader.GetFloat(reader.GetOrdinal(column));
		}

		public static double GetDouble(this SqlDataReader reader, string column)
		{
			CheckType<double>(reader, column);
			return reader.GetDouble(reader.GetOrdinal(column));
		}

		public static bool GetBoolean(this SqlDataReader reader, string column)
		{
			CheckType<bool>(reader, column);
			return reader.GetBoolean(reader.GetOrdinal(column));
		}

		public static string GetString(this SqlDataReader reader, string column)
		{
			CheckType<string>(reader, column);
			return reader.GetString(reader.GetOrdinal(column));
		}

		public static DateTime GetDateTime(this SqlDataReader reader, string column)
		{
			CheckType<DateTime>(reader, column);
			return reader.GetDateTime(reader.GetOrdinal(column));
		}

		public static DateTimeOffset GetDateTimeOffset(this SqlDataReader reader, string column)
		{
			CheckType<DateTimeOffset>(reader, column);
			return reader.GetDateTimeOffset(reader.GetOrdinal(column));
		}

		public static TimeSpan GetTimeSpan(this SqlDataReader reader, string column)
		{
			CheckType<TimeSpan>(reader, column);
			return reader.GetTimeSpan(reader.GetOrdinal(column));
		}

		public static Guid GetGuid(this SqlDataReader reader, string column)
		{
			CheckType<Guid>(reader, column);
			return reader.GetGuid(reader.GetOrdinal(column));
		}

		#endregion

		#region GetXXXXOrNull(COLUMN_INDEX);

		public static int? GetInt32OrNull(this SqlDataReader reader, int index)
		{
			CheckType<int>(reader, index);

			SqlInt32 value = reader.GetSqlInt32(index);
			if (value.IsNull)
			{
				return null;
			}

			return value.Value;
		}

		public static short? GetInt16OrNull(this SqlDataReader reader, int index)
		{
			CheckType<short>(reader, index);

			SqlInt16 value = reader.GetSqlInt16(index);
			if (value.IsNull)
			{
				return null;
			}

			return value.Value;
		}

		public static long? GetInt64OrNull(this SqlDataReader reader, int index)
		{
			CheckType<long>(reader, index);

			SqlInt64 value = reader.GetSqlInt64(index);
			if (value.IsNull)
			{
				return null;
			}

			return value.Value;
		}

		public static decimal? GetDecimalOrNull(this SqlDataReader reader, int index)
		{
			CheckType<decimal>(reader, index);

			SqlDecimal value = reader.GetSqlDecimal(index);
			if (value.IsNull)
			{
				return null;
			}

			return value.Value;
		}

		public static float? GetFloatOrNull(this SqlDataReader reader, int index)
		{
			CheckType<float>(reader, index);

			SqlSingle value = reader.GetSqlSingle(index);
			if (value.IsNull)
			{
				return null;
			}

			return value.Value;
		}

		public static double? GetDoubleOrNull(this SqlDataReader reader, int index)
		{
			CheckType<double>(reader, index);

			SqlDouble value = reader.GetSqlDouble(index);
			if (value.IsNull)
			{
				return null;
			}

			return value.Value;
		}

		public static bool? GetBooleanOrNull(this SqlDataReader reader, int index)
		{
			CheckType<bool>(reader, index);

			SqlBoolean value = reader.GetSqlBoolean(index);
			if (value.IsNull)
			{
				return null;
			}

			return value.Value;
		}

		public static byte? GetByteOrNull(this SqlDataReader reader, int index)
		{
			CheckType<byte>(reader, index);

			SqlByte value = reader.GetSqlByte(index);
			if (value.IsNull)
			{
				return null;
			}

			return value.Value;
		}

		public static byte[] GetBytesOrNull(this SqlDataReader reader, int index)
		{
			CheckType<byte>(reader, index);

			SqlBinary value = reader.GetSqlBinary(index);
			if (value.IsNull)
			{
				return null;
			}

			return value.Value;
		}

		public static string GetStringOrNull(this SqlDataReader reader, int index)
		{
			CheckType<string>(reader, index);

			SqlString value = reader.GetSqlString(index);
			if (value.IsNull)
			{
				return null;
			}

			return value.Value;
		}

		public static DateTime? GetDateTimeOrNull(this SqlDataReader reader, int index)
		{
			CheckType<DateTime>(reader, index);

			if (reader.IsDBNull(index))
			{
				return null;
			}

			return reader.GetDateTime(index);
		}

		public static DateTimeOffset? GetDateTimeOffsetOrNull(this SqlDataReader reader, int index)
		{
			CheckType<DateTimeOffset>(reader, index);

			if (reader.IsDBNull(index))
			{
				return null;
			}

			return reader.GetDateTimeOffset(index);
		}

		public static TimeSpan? GetTimeSpanOrNull(this SqlDataReader reader, int index)
		{
			CheckType<TimeSpan>(reader, index);

			if (reader.IsDBNull(index))
			{
				return null;
			}

			return reader.GetTimeSpan(index);
		}

		public static Guid? GetGuidOrNull(this SqlDataReader reader, int index)
		{
			CheckType<Guid>(reader, index);

			if (reader.IsDBNull(index))
			{
				return null;
			}
			return reader.GetGuid(index);
		}

		#endregion

		#region GetXXXXOrNull("COLUMN_NAME");

		public static int? GetInt32OrNull(this SqlDataReader reader, string column)
		{
			return GetInt32OrNull(reader, reader.GetOrdinal(column));
		}

		public static short? GetInt16OrNull(this SqlDataReader reader, string column)
		{
			return GetInt16OrNull(reader, reader.GetOrdinal(column));
		}

		public static long? GetInt64OrNull(this SqlDataReader reader, string column)
		{
			return GetInt64OrNull(reader, reader.GetOrdinal(column));
		}

		public static decimal? GetDecimalOrNull(this SqlDataReader reader, string column)
		{
			return GetDecimalOrNull(reader, reader.GetOrdinal(column));
		}

		public static float? GetFloatOrNull(this SqlDataReader reader, string column)
		{
			return GetFloatOrNull(reader, reader.GetOrdinal(column));
		}

		public static double? GetDoubleOrNull(this SqlDataReader reader, string column)
		{
			return GetDoubleOrNull(reader, reader.GetOrdinal(column));
		}

		public static bool? GetBooleanOrNull(this SqlDataReader reader, string column)
		{
			return GetBooleanOrNull(reader, reader.GetOrdinal(column));
		}

		public static string GetStringOrNull(this SqlDataReader reader, string column)
		{
			return GetStringOrNull(reader, reader.GetOrdinal(column));
		}

		public static byte? GetByteOrNull(this SqlDataReader reader, string column)
		{
			return GetByteOrNull(reader, reader.GetOrdinal(column));
		}

		public static byte[] GetBytesOrNull(this SqlDataReader reader, string column)
		{
			return GetBytesOrNull(reader, reader.GetOrdinal(column));
		}

		public static DateTime? GetDateTimeOrNull(this SqlDataReader reader, string column)
		{
			return GetDateTimeOrNull(reader, reader.GetOrdinal(column));
		}

		public static DateTimeOffset? GetDateTimeOffsetOrNull(this SqlDataReader reader, string column)
		{
			return GetDateTimeOffsetOrNull(reader, reader.GetOrdinal(column));
		}

		public static TimeSpan? GetTimeSpanOrNull(this SqlDataReader reader, string column)
		{
			return GetTimeSpanOrNull(reader, reader.GetOrdinal(column));
		}

		public static Guid? GetGuidOrNull(this SqlDataReader reader, string column)
		{
			return GetGuidOrNull(reader, reader.GetOrdinal(column));
		}

		#endregion

		#region GetXXXXOrDefault(COLUMN_INDEX, DEFAULT_VALUE);

		public static int GetInt32OrDefault(this SqlDataReader reader, int index, int defaultValue = 0)
		{
			CheckType<int>(reader, index);

			SqlInt32 value = reader.GetSqlInt32(index);
			if (value.IsNull)
			{
				return defaultValue;
			}

			return value.Value;
		}

		public static short GetInt16OrDefault(this SqlDataReader reader, int index, short defaultValue = 0)
		{
			CheckType<short>(reader, index);

			SqlInt16 value = reader.GetSqlInt16(index);
			if (value.IsNull)
			{
				return defaultValue;
			}

			return value.Value;
		}

		public static long GetInt64OrDefault(this SqlDataReader reader, int index, long defaultValue = 0)
		{
			CheckType<long>(reader, index);

			SqlInt64 value = reader.GetSqlInt64(index);
			if (value.IsNull)
			{
				return defaultValue;
			}

			return value.Value;
		}

		public static decimal GetDecimalOrDefault(this SqlDataReader reader, int index, decimal defaultValue = 0M)
		{
			CheckType<decimal>(reader, index);

			SqlDecimal value = reader.GetSqlDecimal(index);
			if (value.IsNull)
			{
				return defaultValue;
			}

			return value.Value;
		}

		public static float GetFloatOrDefault(this SqlDataReader reader, int index, float defaultValue = 0F)
		{
			CheckType<float>(reader, index);

			SqlSingle value = reader.GetSqlSingle(index);
			if (value.IsNull)
			{
				return defaultValue;
			}

			return value.Value;
		}

		public static double GetDoubleOrDefault(this SqlDataReader reader, int index, double defaultValue = 0D)
		{
			CheckType<double>(reader, index);

			SqlDouble value = reader.GetSqlDouble(index);
			if (value.IsNull)
			{
				return defaultValue;
			}

			return value.Value;
		}

		public static bool GetBooleanOrDefault(this SqlDataReader reader, int index, bool defaultValue = false)
		{
			CheckType<bool>(reader, index);

			SqlBoolean value = reader.GetSqlBoolean(index);
			if (value.IsNull)
			{
				return defaultValue;
			}

			return value.Value;
		}

		public static string GetStringOrDefault(this SqlDataReader reader, int index, string defaultValue = "")
		{
			CheckType<string>(reader, index);

			SqlString value = reader.GetSqlString(index);
			if (value.IsNull)
			{
				return defaultValue;
			}

			return value.Value;
		}

		public static DateTime GetDateTimeOrDefault(this SqlDataReader reader, int index)
		{
			return GetDateTimeOrDefault(reader, index, DateTime.MinValue);
		}

		public static DateTime GetDateTimeOrDefault(this SqlDataReader reader, int index, DateTime defaultValue)
		{
			CheckType<DateTime>(reader, index);

			if (reader.IsDBNull(index))
			{
				return defaultValue;
			}

			return reader.GetDateTime(index);
		}

		public static DateTimeOffset GetDateTimeOffsetOrDefault(this SqlDataReader reader, int index)
		{
			return GetDateTimeOffsetOrDefault(reader, index, DateTimeOffset.MinValue);
		}

		public static DateTimeOffset GetDateTimeOffsetOrDefault(this SqlDataReader reader, int index, DateTimeOffset defaultValue)
		{
			CheckType<DateTimeOffset>(reader, index);

			if (reader.IsDBNull(index))
			{
				return defaultValue;
			}

			return reader.GetDateTimeOffset(index);
		}

		public static TimeSpan GetTimeSpanOrDefault(this SqlDataReader reader, int index)
		{
			return GetTimeSpanOrDefault(reader, index, TimeSpan.Zero);
		}

		public static TimeSpan GetTimeSpanOrDefault(this SqlDataReader reader, int index, TimeSpan defaultValue)
		{
			CheckType<TimeSpan>(reader, index);

			if (reader.IsDBNull(index))
			{
				return defaultValue;
			}

			return reader.GetTimeSpan(index);
		}

		public static Guid GetGuidOrDefault(this SqlDataReader reader, int index, Guid defaultValue)
		{
			CheckType<TimeSpan>(reader, index);

			if (reader.IsDBNull(index))
			{
				return defaultValue;
			}

			return reader.GetGuid(index);
		}

		#endregion

		#region GetXXXXOrDefault("COLUMN_NAME", DEFAULT_VALUE);

		public static int GetInt32OrDefault(this SqlDataReader reader, string column, int defaultValue = 0)
		{
			return GetInt32OrDefault(reader, reader.GetOrdinal(column), defaultValue);
		}

		public static short GetInt16OrDefault(this SqlDataReader reader, string column, short defaultValue = 0)
		{
			return GetInt16OrDefault(reader, reader.GetOrdinal(column), defaultValue);
		}

		public static long GetInt64OrDefault(this SqlDataReader reader, string column, long defaultValue = 0)
		{
			return GetInt64OrDefault(reader, reader.GetOrdinal(column), defaultValue);
		}

		public static decimal GetDecimalOrDefault(this SqlDataReader reader, string column, decimal defaultValue = 0M)
		{
			return GetDecimalOrDefault(reader, reader.GetOrdinal(column), defaultValue);
		}

		public static float GetFloatOrDefault(this SqlDataReader reader, string column, float defaultValue = 0F)
		{
			return GetFloatOrDefault(reader, reader.GetOrdinal(column), defaultValue);
		}

		public static double GetDoubleOrDefault(this SqlDataReader reader, string column, double defaultValue = 0D)
		{
			return GetDoubleOrDefault(reader, reader.GetOrdinal(column), defaultValue);
		}

		public static bool GetBooleanOrDefault(this SqlDataReader reader, string column, bool defaultValue = false)
		{
			return GetBooleanOrDefault(reader, reader.GetOrdinal(column), defaultValue);
		}

		public static string GetStringOrDefault(this SqlDataReader reader, string column, string defaultValue = "")
		{
			return GetStringOrDefault(reader, reader.GetOrdinal(column), defaultValue);
		}

		public static DateTime GetDateTimeOrDefault(this SqlDataReader reader, string column)
		{
			return GetDateTimeOrDefault(reader, reader.GetOrdinal(column));
		}

		public static DateTime GetDateTimeOrDefault(this SqlDataReader reader, string column, DateTime defaultValue)
		{
			return GetDateTimeOrDefault(reader, reader.GetOrdinal(column), defaultValue);
		}

		public static DateTimeOffset GetDateTimeOffsetOrDefault(this SqlDataReader reader, string column)
		{
			return GetDateTimeOffsetOrDefault(reader, reader.GetOrdinal(column));
		}

		public static DateTimeOffset GetDateTimeOffsetOrDefault(this SqlDataReader reader, string column, DateTimeOffset defaultValue)
		{
			return GetDateTimeOffsetOrDefault(reader, reader.GetOrdinal(column), defaultValue);
		}

		public static TimeSpan GetTimeSpanOrDefault(this SqlDataReader reader, string column)
		{
			return GetTimeSpanOrDefault(reader, reader.GetOrdinal(column));
		}

		public static TimeSpan GetTimeSpanOrDefault(this SqlDataReader reader, string column, TimeSpan defaultValue)
		{
			return GetTimeSpanOrDefault(reader, reader.GetOrdinal(column), defaultValue);
		}

		public static Guid GetGuidOrDefault(this SqlDataReader reader, string column, Guid defaultValue)
		{
			return GetGuidOrDefault(reader, reader.GetOrdinal(column), defaultValue);
		}

		#endregion

		[Conditional("DEBUG")]
		private static void CheckType<TExpectedType>(SqlDataReader reader, string column)
		{

			var columnIndex = reader.GetOrdinal(column);
			CheckType<TExpectedType>(reader, columnIndex);
		}

		[Conditional("DEBUG")]
		private static void CheckType<TExpectedType>(SqlDataReader reader, int columnIndex)
		{
			string column = reader.GetName(columnIndex);
			if (reader.GetFieldType(columnIndex) != typeof(TExpectedType))
			{
				throw new InvalidOperationException(string.Format(
						"Error while getting column '{0}' (db type: {1}). Expected type '{2}', but actual is '{3}'",
						column, reader.GetDataTypeName(columnIndex), typeof(TExpectedType), reader.GetFieldType(columnIndex)));
			}
		}
	}
}
