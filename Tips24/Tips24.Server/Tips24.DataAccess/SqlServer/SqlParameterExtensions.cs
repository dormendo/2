using System;
using System.Data;
using System.Data.SqlClient;

namespace Tips24
{
	public static class SqlParameterExtensions
	{
		public static SqlParameter Output(this SqlParameter parameter)
		{
			parameter.Direction = ParameterDirection.Output;
			return parameter;
		}

		public static SqlParameter InputOutput(this SqlParameter parameter)
		{
			parameter.Direction = ParameterDirection.InputOutput;
			return parameter;
		}

		public static SqlParameter SetValue<T>(this SqlParameter parameter, Nullable<T> value) where T : struct
		{
			if (value.HasValue)
			{
				parameter.Value = value.Value;
			}
			else
			{
				parameter.Value = DBNull.Value;
			}

			return parameter;
		}

		public static SqlParameter SetValue(this SqlParameter parameter, object value)
		{
			if (value != null)
			{
				parameter.Value = value;
			}
			else
			{
				parameter.Value = DBNull.Value;
			}

			return parameter;
		}

		#region GetXXXX()

		public static int GetInt32(this SqlParameter param)
		{
			return Convert.ToInt32(param.Value);
		}

		public static short GetInt16(this SqlParameter param)
		{
			return Convert.ToInt16(param.Value);
		}

		public static long GetInt64(this SqlParameter param)
		{
			return Convert.ToInt64(param.Value);
		}

		public static decimal GetDecimal(this SqlParameter param)
		{
			return Convert.ToDecimal(param.Value);
		}

		public static float GetFloat(this SqlParameter param)
		{
			return Convert.ToSingle(param.Value);
		}

		public static double GetDouble(this SqlParameter param)
		{
			return Convert.ToDouble(param.Value);
		}

		public static bool GetBoolean(this SqlParameter param)
		{
			return Convert.ToBoolean(param.Value);
		}

		public static string GetString(this SqlParameter param)
		{
			return Convert.ToString(param.Value);
		}

		public static DateTime GetDateTime(this SqlParameter param)
		{
			return Convert.ToDateTime(param.Value);
		}

		public static TimeSpan GetTimeSpan(this SqlParameter param)
		{
			return (TimeSpan)param.Value;
		}

		public static char GetChar(this SqlParameter param)
		{
			return Convert.ToChar(param.Value);
		}

		public static byte GetByte(this SqlParameter param)
		{
			return Convert.ToByte(param.Value);
		}

		#endregion

		#region GetXXXXOrNull()

		public static int? GetInt32OrNull(this SqlParameter param)
		{
			return param.Value == DBNull.Value ? (int?)null : Convert.ToInt32(param.Value);
		}

		public static short? GetInt16OrNull(this SqlParameter param)
		{
			return param.Value == DBNull.Value ? (short?)null : Convert.ToInt16(param.Value);
		}

		public static long? GetInt64OrNull(this SqlParameter param)
		{
			return param.Value == DBNull.Value ? (long?)null : Convert.ToInt64(param.Value);
		}

		public static decimal? GetDecimalOrNull(this SqlParameter param)
		{
			return param.Value == DBNull.Value ? (decimal?)null : Convert.ToDecimal(param.Value);
		}

		public static float? GetFloatOrNull(this SqlParameter param)
		{
			return param.Value == DBNull.Value ? (float?)null : Convert.ToSingle(param.Value);
		}

		public static double? GetDoubleOrNull(this SqlParameter param)
		{
			return param.Value == DBNull.Value ? (double?)null : Convert.ToDouble(param.Value);
		}

		public static bool? GetBooleanOrNull(this SqlParameter param)
		{
			return param.Value == DBNull.Value ? (bool?)null : Convert.ToBoolean(param.Value);
		}

		public static string GetStringOrNull(this SqlParameter param)
		{
			return param.Value == DBNull.Value ? (string)null : Convert.ToString(param.Value);
		}

		public static DateTime? GetDateTimeOrNull(this SqlParameter param)
		{
			return param.Value == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(param.Value);
		}

		public static TimeSpan? GetTimeSpanOrNull(this SqlParameter param)
		{
			return param.Value == DBNull.Value ? (TimeSpan?)null : (TimeSpan)param.Value;
		}

		public static byte? GetByteOrNull(this SqlParameter param)
		{
			return param.Value == DBNull.Value ? (byte?)null : (byte)param.Value;
		}

		#endregion

		#region GetXXXXOrDefault()

		public static int GetInt32OrDefault(this SqlParameter param, int defaultValue = 0)
		{
			return param.Value == DBNull.Value ? defaultValue : Convert.ToInt32(param.Value);
		}

		public static short GetInt16OrDefault(this SqlParameter param, short defaultValue = 0)
		{
			return param.Value == DBNull.Value ? defaultValue : Convert.ToInt16(param.Value);
		}

		public static long GetInt64OrDefault(this SqlParameter param, long defaultValue = 0)
		{
			return param.Value == DBNull.Value ? defaultValue : Convert.ToInt64(param.Value);
		}

		public static decimal GetDecimalOrDefault(this SqlParameter param, decimal defaultValue = 0M)
		{
			return param.Value == DBNull.Value ? defaultValue : Convert.ToDecimal(param.Value);
		}

		public static float GetFloatOrDefault(this SqlParameter param, float defaultValue = 0F)
		{
			return param.Value == DBNull.Value ? defaultValue : Convert.ToSingle(param.Value);
		}

		public static double GetDoubleOrDefault(this SqlParameter param, double defaultValue = 0D)
		{
			return param.Value == DBNull.Value ? defaultValue : Convert.ToDouble(param.Value);
		}

		public static bool GetBooleanOrDefault(this SqlParameter param, bool defaultValue = false)
		{
			return param.Value == DBNull.Value ? defaultValue : Convert.ToBoolean(param.Value);
		}

		public static string GetStringOrDefault(this SqlParameter param, string defaultValue = "")
		{
			return param.Value == DBNull.Value ? defaultValue : Convert.ToString(param.Value);
		}

		public static DateTime GetDateTimeOrDefault(this SqlParameter param)
		{
			return GetDateTimeOrDefault(param, DateTime.MinValue);
		}

		public static DateTime GetDateTimeOrDefault(this SqlParameter param, DateTime defaultValue)
		{
			return param.Value == DBNull.Value ? defaultValue : Convert.ToDateTime(param.Value);
		}

		public static TimeSpan GetTimeSpanOrDefault(this SqlParameter param)
		{
			return GetTimeSpanOrDefault(param, TimeSpan.Zero);
		}

		public static TimeSpan GetTimeSpanOrDefault(this SqlParameter param, TimeSpan defaultValue)
		{
			return param.Value == DBNull.Value ? defaultValue : (TimeSpan)param.Value;
		}

		public static byte GetByteOrDefault(this SqlParameter param, byte defaultValue = 0)
		{
			return param.Value == DBNull.Value ? defaultValue : (byte)param.Value;
		}

		#endregion
	}
}
