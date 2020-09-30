// Decompiled with JetBrains decompiler
// Type: AutomateCommandCreation.SqlParamMetaData
// Assembly: AutomateCommandCreation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D80C71A9-7029-489E-8398-6B27AD4E17DC
// Assembly location: D:\Storage\nagelfar@mail.ru\Music\betting-system\Project Tools\AutomateCommandCreation\AutomateCommandCreation.exe

using System.Collections.Generic;
using System.Data;

namespace AutomateCommandCreation
{
	public class SqlParamMetaData
	{
		public static Dictionary<SqlDbType, ParamTypeMeta> Items { get; private set; }

		static SqlParamMetaData()
		{
			SqlParamMetaData.Items = new Dictionary<SqlDbType, ParamTypeMeta>();
			SqlParamMetaData.Items.Add(SqlDbType.Binary, new ParamTypeMeta("binary", "AddVarBinaryMaxParam(\"{0}\"{1})", "byte[]", "GetBytes({0})", "(byte[]){0}.Value", ""));
			SqlParamMetaData.Items.Add(SqlDbType.VarBinary, new ParamTypeMeta("varbinary", "AddVarBinaryMaxParam(\"{0}\"{1})", "byte[]", "GetBytes({0})", "(byte[]){0}.Value", ""));
			SqlParamMetaData.Items.Add(SqlDbType.Char, new ParamTypeMeta("char", "AddCharParam(\"{0}\", {2}{1})", "string", "GetStringOrDefault({0})", "{0}.Value.ToString()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.NChar, new ParamTypeMeta("nchar", "AddNCharParam(\"{0}\", {2}{1})", "string", "GetStringOrDefault({0})", "{0}.Value.ToString()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.Date, new ParamTypeMeta("date", "AddDateParam(\"{0}\"{1})", "DateTime", "GetDateTimeOrNull({0})", "{0}.GetDateTimeOrNull()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.Decimal, new ParamTypeMeta("decimal", "AddDecimalParam(\"{0}\", {3}, {4}{1})", "decimal", "GetDecimalOrDefault({0})", "{0}.GetDecimalOrDefault()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.Money, new ParamTypeMeta("money", "AddDecimalParam(\"{0}\", {3}, {4}{1})", "decimal", "GetDecimalOrDefault({0})", "{0}.GetDecimalOrDefault()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.Float, new ParamTypeMeta("float", "AddFloatParam(\"{0}\"{1})", "double", "GetDoubleOrDefault({0})", "{0}.GetDoubleOrDefault()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.Real, new ParamTypeMeta("real", "AddRealParam(\"{0}\"{1})", "float", "GetFloatOrDefault({0})", "{0}.GetFloatOrDefault()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.BigInt, new ParamTypeMeta("bigint", "AddBigIntParam(\"{0}\"{1})", "long", "GetInt64OrDefault({0})", "{0}.GetInt64OrDefault()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.Int, new ParamTypeMeta("int", "AddIntParam(\"{0}\"{1})", "int", "GetInt32OrDefault({0})", "{0}.GetInt32OrDefault()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.SmallInt, new ParamTypeMeta("smallint", "AddSmallIntParam(\"{0}\"{1})", "short", "GetInt16OrDefault({0})", "{0}.GetInt16OrDefault()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.TinyInt, new ParamTypeMeta("tinyint", "AddTinyIntParam(\"{0}\"{1})", "byte", "GetByteOrDefault({0})", "{0}.GetByteOrDefault()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.Time, new ParamTypeMeta("time", "AddTimeParam(\"{0}\"{1})", "TimeSpan", "GetTimeSpanOrDefault({0})", "{0}.GetTimeSpanOrDefault()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.Timestamp, new ParamTypeMeta("timestamp", "AddDateTime2Param(\"{0}\"{1})", "DateTime", "GetDateTimeOrNull({0})", "{0}.GetDateTimeOrNull()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.DateTime, new ParamTypeMeta("datetime", "AddDateTimeParam(\"{0}\"{1})", "DateTime", "GetDateTimeOrNull({0})", "{0}.GetDateTimeOrNull()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.DateTime2, new ParamTypeMeta("datetime2", "AddDateTime2Param(\"{0}\"{1})", "DateTime", "GetDateTimeOrNull({0})", "{0}.GetDateTimeOrNull()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.DateTimeOffset, new ParamTypeMeta("datetimeoffset", "AddDateTimeOffsetParam(\"{0}\"{1})", "DateTimeOffset", "GetDateTimeOffsetOrNull({0})", "{0}.GetDateTimeOffsetOrNull()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.VarChar, new ParamTypeMeta("varchar", "AddVarCharParam(\"{0}\", {2}{1})", "string", "GetStringOrDefault({0})", "{0}.Value.ToString()", "AddVarCharMaxParam(\"{0}\"{1})"));
			SqlParamMetaData.Items.Add(SqlDbType.NVarChar, new ParamTypeMeta("nvarchar", "AddNVarCharParam(\"{0}\", {2}{1})", "string", "GetStringOrDefault({0})", "{0}.Value.ToString()", "AddNVarCharMaxParam(\"{0}\"{1})"));
			SqlParamMetaData.Items.Add(SqlDbType.Bit, new ParamTypeMeta("bit", "AddBitParam(\"{0}\"{1})", "bool", "GetBooleanOrDefault({0})", "{0}.GetBooleanOrDefault()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.UniqueIdentifier, new ParamTypeMeta("uniqueidentifier", "AddUniqueIdentifierParam(\"{0}\"{1})", "Guid", "GetGuidOrDefault({0})", "{0}.GetGuidOrDefault()", ""));
			SqlParamMetaData.Items.Add(SqlDbType.Structured, new ParamTypeMeta("dbo.SomeType READONLY", "AddStructuredParam(\"{0}\"{1})", "", "", "", ""));
		}
	}
}
