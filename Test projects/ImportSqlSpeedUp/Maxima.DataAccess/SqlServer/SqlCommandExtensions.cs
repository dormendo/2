using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Maxima
{
    public static class SqlCommandExtensions
    {
        public static async Task<T> GetScalarAsyncOrDefault<T>(this SqlCommand command, T def)
        {
            object value = await command.ExecuteScalarAsync();
            return value is T ? (T)value : def;
        }

        #region AddIntParam
        
        public static SqlParameter AddIntParam(this SqlCommand command, string paramName, int value)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Int, Value = value });
        }

        public static SqlParameter AddIntParam(this SqlCommand command, string paramName)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Int, Value = DBNull.Value });
        }

        public static SqlParameter AddIntParam(this SqlCommand command, string paramName, int? value)
        {
            return value.HasValue ? AddIntParam(command, paramName, value.Value) : AddIntParam(command, paramName);
        }

        #endregion

        #region AddSmallIntParam

        public static SqlParameter AddSmallIntParam(this SqlCommand command, string paramName, short value)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.SmallInt, Value = value });
        }

        public static SqlParameter AddSmallIntParam(this SqlCommand command, string paramName)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.SmallInt, Value = DBNull.Value });
        }

        public static SqlParameter AddSmallIntParam(this SqlCommand command, string paramName, short? value)
        {
            return value.HasValue ? AddSmallIntParam(command, paramName, value.Value) : AddSmallIntParam(command, paramName);
        }

        #endregion

        #region AddTinyIntParam

        public static SqlParameter AddTinyIntParam(this SqlCommand command, string paramName, byte value)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.TinyInt, Value = value });
        }

        public static SqlParameter AddTinyIntParam(this SqlCommand command, string paramName)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.TinyInt, Value = DBNull.Value });
        }

        public static SqlParameter AddTinyIntParam(this SqlCommand command, string paramName, byte? value)
        {
            return value.HasValue ? AddTinyIntParam(command, paramName, value.Value) : AddTinyIntParam(command, paramName);
        }

        #endregion

        #region AddBigIntParam

        public static SqlParameter AddBigIntParam(this SqlCommand command, string paramName, long value)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.BigInt, Value = value });
        }

        public static SqlParameter AddBigIntParam(this SqlCommand command, string paramName)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.BigInt, Value = DBNull.Value });
        }

        public static SqlParameter AddBigIntParam(this SqlCommand command, string paramName, long? value)
        {
            return value.HasValue ? AddBigIntParam(command, paramName, value.Value) : AddBigIntParam(command, paramName);
        }

        #endregion

        #region AddUniqueIdentifierParam

        public static SqlParameter AddUniqueIdentifierParam(this SqlCommand command, string paramName, Guid value)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.UniqueIdentifier, Value = value });
        }

        public static SqlParameter AddUniqueIdentifierParam(this SqlCommand command, string paramName)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.UniqueIdentifier, Value = DBNull.Value });
        }

        public static SqlParameter AddUniqueIdentifierParam(this SqlCommand command, string paramName, Guid? value)
        {
            return value.HasValue ? AddUniqueIdentifierParam(command, paramName, value.Value) : AddUniqueIdentifierParam(command, paramName);
        }

        #endregion

        #region AddVarCharParam

        public static SqlParameter AddVarCharParam(this SqlCommand command, string paramName, int size, string value)
        {
            if (value == null)
            {
                return AddVarCharParam(command, paramName, size);
            }
            else
            {
                CheckParamSize(paramName, size, value);

                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.VarChar, Size = size, Value = value });
            }
        }

        public static SqlParameter AddVarCharParam(this SqlCommand command, string paramName, int size)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.VarChar, Size = size, Value = DBNull.Value });
        }

        public static SqlParameter AddVarCharParam(this SqlCommand command, string paramName, string value)
        {
            if (value == null)
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.VarChar, Value = DBNull.Value });
            }
            else
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.VarChar, Value = value });
            }
        }

        #endregion

        #region AddNVarCharParam

        public static SqlParameter AddNVarCharParam(this SqlCommand command, string paramName, int size, string value)
        {
            if (value == null)
            {
                return AddNVarCharParam(command, paramName, size);
            }
            else
            {
                CheckParamSize(paramName, size, value);

                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.NVarChar, Size = size, Value = value });
            }
        }

        public static SqlParameter AddNVarCharParam(this SqlCommand command, string paramName, int size)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.NVarChar, Size = size, Value = DBNull.Value });
        }

        public static SqlParameter AddSysNameParam(this SqlCommand command, string paramName, string value)
        {
            if (value == null)
            {
                return AddNVarCharParam(command, paramName, 256);
            }
            else
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.NVarChar, Size = 256, Value = value });
            }
        }

        public static SqlParameter AddNVarCharParam(this SqlCommand command, string paramName, string value)
        {
            if (value == null)
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.NVarChar, Value = DBNull.Value });
            }
            else
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.NVarChar, Value = value });
            }
        }

        #endregion

        #region AddVarCharMaxParam

        public static SqlParameter AddVarCharMaxParam(this SqlCommand command, string paramName, string value)
        {
            if (value == null)
            {
                return AddVarCharMaxParam(command, paramName);
            }
            else
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.VarChar, Size = -1, Value = value });
            }
        }

        public static SqlParameter AddVarCharMaxParam(this SqlCommand command, string paramName)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.VarChar, Size = -1, Value = DBNull.Value });
        }

        #endregion

        #region AddNVarCharMaxParam

        public static SqlParameter AddNVarCharMaxParam(this SqlCommand command, string paramName, string value)
        {
            if (value == null)
            {
                return AddNVarCharMaxParam(command, paramName);
            }
            else
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.NVarChar, Size = -1, Value = value });
            }
        }

        public static SqlParameter AddNVarCharMaxParam(this SqlCommand command, string paramName)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.NVarChar, Size = -1, Value = DBNull.Value });
        }

        #endregion

        #region AddCharParam

        public static SqlParameter AddCharParam(this SqlCommand command, string paramName, int size, string value)
        {
            if (value == null)
            {
                return AddCharParam(command, paramName, size);
            }
            else
            {
                CheckParamSize(paramName, size, value);

                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Char, Size = size, Value = value });
            }
        }

        public static SqlParameter AddCharParam(this SqlCommand command, string paramName, int size)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Char, Size = size, Value = DBNull.Value });
        }

        public static SqlParameter AddCharParam(this SqlCommand command, string paramName, string value)
        {
            if (value == null)
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Char, Value = DBNull.Value });
            }
            else
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Char, Value = value });
            }
        }

        #endregion

        #region AddNCharParam

        public static SqlParameter AddNCharParam(this SqlCommand command, string paramName, int size, string value)
        {
            if (value == null)
            {
                return AddNCharParam(command, paramName, size);
            }
            else
            {
                CheckParamSize(paramName, size, value);

                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.NChar, Size = size, Value = value });
            }
        }

        public static SqlParameter AddNCharParam(this SqlCommand command, string paramName, int size)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.NChar, Size = size, Value = DBNull.Value });
        }

        public static SqlParameter AddNCharParam(this SqlCommand command, string paramName, string value)
        {
            if (value == null)
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.NChar, Value = DBNull.Value });
            }
            else
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.NChar, Value = value });
            }
        }

        #endregion

        #region AddDateTimeParam

        public static SqlParameter AddDateTimeParam(this SqlCommand command, string paramName, DateTime value)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.DateTime, Value = value });
        }

        public static SqlParameter AddDateTimeParam(this SqlCommand command, string paramName)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.DateTime, Value = DBNull.Value });
        }

        public static SqlParameter AddDateTimeParam(this SqlCommand command, string paramName, DateTime? value)
        {
            return value.HasValue ? AddDateTimeParam(command, paramName, value.Value) : AddDateTimeParam(command, paramName);
        }

        #endregion

        #region AddDateParam

        public static SqlParameter AddDateParam(this SqlCommand command, string paramName, DateTime value)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.DateTime, Value = value.Date });
        }

        public static SqlParameter AddDateParam(this SqlCommand command, string paramName)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.DateTime, Value = DBNull.Value });
        }

        public static SqlParameter AddDateParam(this SqlCommand command, string paramName, DateTime? value)
        {
            return value.HasValue ? AddDateParam(command, paramName, value.Value.Date) : AddDateParam(command, paramName);
        }

        #endregion

        #region AddTimeParam

        public static SqlParameter AddTimeParam(this SqlCommand command, string paramName, TimeSpan value)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Time, Value = value });
        }

        public static SqlParameter AddTimeParam(this SqlCommand command, string paramName)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Time, Value = DBNull.Value });
        }

        public static SqlParameter AddTimeParam(this SqlCommand command, string paramName, TimeSpan? value)
        {
            return value.HasValue ? AddTimeParam(command, paramName, value.Value) : AddTimeParam(command, paramName);
        }

        #endregion

        #region AddDateTime2Param

        public static SqlParameter AddDateTime2Param(this SqlCommand command, string paramName, DateTime value)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.DateTime2, Value = value });
        }

        public static SqlParameter AddDateTime2Param(this SqlCommand command, string paramName)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.DateTime2, Value = DBNull.Value });
        }

        public static SqlParameter AddDateTime2Param(this SqlCommand command, string paramName, DateTime? value)
        {
            return value.HasValue ? AddDateTimeParam(command, paramName, value.Value) : AddDateTimeParam(command, paramName);
        }

        public static SqlParameter AddDateTime2Param(this SqlCommand command, string paramName, byte scale, DateTime value)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.DateTime2, Scale = scale, Value = value });
        }

        public static SqlParameter AddDateTime2Param(this SqlCommand command, string paramName, byte scale)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.DateTime2, Scale = scale, Value = DBNull.Value });
        }

        public static SqlParameter AddDateTime2Param(this SqlCommand command, string paramName, byte scale, DateTime? value)
        {
            return value.HasValue ? AddDateTime2Param(command, paramName, scale, value.Value) : AddDateTime2Param(command, paramName, scale);
        }

        #endregion

        #region AddDecimalParam

        public static SqlParameter AddDecimalParam(this SqlCommand command, string paramName, byte precision, byte scale, decimal value)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Decimal, Precision = precision, Scale = scale, Value = value });
        }

        public static SqlParameter AddDecimalParam(this SqlCommand command, string paramName, byte precision, byte scale)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Decimal, Precision = precision, Scale = scale, Value = DBNull.Value });
        }

        public static SqlParameter AddDecimalParam(this SqlCommand command, string paramName, byte precision, byte scale, decimal? value)
        {
            return value.HasValue ? AddDecimalParam(command, paramName, precision, scale, value.Value) : AddDecimalParam(command, paramName, precision, scale);
        }

        #endregion

        #region AddBitParam

        public static SqlParameter AddBitParam(this SqlCommand command, string paramName, bool value)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Bit, Value = value });
        }

        public static SqlParameter AddBitParam(this SqlCommand command, string paramName)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Bit, Value = DBNull.Value });
        }

        public static SqlParameter AddBitParam(this SqlCommand command, string paramName, bool? value)
        {
            return value.HasValue ? AddBitParam(command, paramName, value.Value) : AddBitParam(command, paramName);
        }

        #endregion

        #region AddVarBinaryParam

        public static SqlParameter AddVarBinaryParam(this SqlCommand command, string paramName, int size, byte[] value)
        {
            if (value == null)
            {
                return AddVarBinaryParam(command, paramName, size);
            }
            else
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.VarBinary, Size = size, Value = value });
            }
        }

        public static SqlParameter AddVarBinaryParam(this SqlCommand command, string paramName, int size)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.VarBinary, Size = size, Value = DBNull.Value });
        }

        public static SqlParameter AddVarBinaryParam(this SqlCommand command, string paramName, byte[] value)
        {
            if (value == null)
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.VarBinary, Value = DBNull.Value });
            }
            else
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.VarBinary, Value = value });
            }
        }

        #endregion

        #region AddVarBinaryMaxParam

        public static SqlParameter AddVarBinaryMaxParam(this SqlCommand command, string paramName, byte[] value)
        {
            if (value == null)
            {
                return AddVarBinaryMaxParam(command, paramName);
            }
            else
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.VarBinary, Size = -1, Value = value });
            }
        }

        public static SqlParameter AddVarBinaryMaxParam(this SqlCommand command, string paramName)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.VarBinary, Size = -1, Value = DBNull.Value });
        }

        #endregion

        #region AddBinaryParam

        public static SqlParameter AddBinaryParam(this SqlCommand command, string paramName, int size, byte[] value)
        {
            if (value == null)
            {
                return AddBinaryParam(command, paramName, size);
            }
            else
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Binary, Size = size, Value = value });
            }
        }

        public static SqlParameter AddBinaryParam(this SqlCommand command, string paramName, int size)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Binary, Size = size, Value = DBNull.Value });
        }

        public static SqlParameter AddBinaryParam(this SqlCommand command, string paramName, byte[] value)
        {
            if (value == null)
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Binary, Value = DBNull.Value });
            }
            else
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Binary, Value = value });
            }
        }

        #endregion

        #region AddStructuredParam

        public static SqlParameter AddStructuredParam(this SqlCommand command, string paramName, string typeName, StructuredParamValue value)
        {
            if (value == null)
            {
                return AddStructuredParam(command, paramName, typeName);
            }
            else
            {
                return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Structured, TypeName = typeName, Value = value });
            }
        }

        public static SqlParameter AddStructuredParam(this SqlCommand command, string paramName, string typeName)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Structured, TypeName = typeName, Value = DBNull.Value });
        }

        #endregion

        #region AddReturnValue

        public static SqlParameter AddReturnValue(this SqlCommand command, string paramName = "@returnValue")
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Int, Direction = ParameterDirection.ReturnValue });
        }

        #endregion

        #region AddFloatParam

        public static SqlParameter AddFloatParam(this SqlCommand command, string paramName, double value)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Float, Value = value });
        }

        public static SqlParameter AddFloatParam(this SqlCommand command, string paramName)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Float, Value = DBNull.Value });
        }

        public static SqlParameter AddFloatParam(this SqlCommand command, string paramName, byte precision, byte scale, double? value)
        {
            return value.HasValue ? AddFloatParam(command, paramName, value.Value) : AddFloatParam(command, paramName);
        }

        #endregion

        #region AddRealParam

        public static SqlParameter AddRealParam(this SqlCommand command, string paramName, float value)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Real, Value = value });
        }

        public static SqlParameter AddRealParam(this SqlCommand command, string paramName)
        {
            return command.Parameters.Add(new SqlParameter { ParameterName = paramName, SqlDbType = SqlDbType.Real, Value = DBNull.Value });
        }

        public static SqlParameter AddRealParam(this SqlCommand command, string paramName, byte precision, byte scale, float? value)
        {
            return value.HasValue ? AddRealParam(command, paramName, value.Value) : AddRealParam(command, paramName);
        }

        #endregion

        #region Timeout

        public static SqlCommand Timeout(this SqlCommand command, int timeout)
        {
            command.CommandTimeout = timeout;
            return command;
        }

        #endregion

        /// <summary>
        /// Проверка длины значения передаваемого в параметр <see cref="paramName"/>
        /// </summary>
        /// <param name="paramName">Имя проверяемого параметра</param>
        /// <param name="size">Максимальная длина параметра в хранимой процедуре</param>
        /// <param name="value">Значение параметра</param>
        private static void CheckParamSize(string paramName, int size, string value)
        {
            if (!string.IsNullOrEmpty(value) && size > 0 && value.Length > size)
            {
                throw new ArgumentException(string.Format("Длина значения параметра {0} превышает максимальную длину, определенную в хранимой процедуре", paramName));
            }
        }
    }
}
