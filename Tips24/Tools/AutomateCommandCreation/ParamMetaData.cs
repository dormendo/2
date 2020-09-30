// Decompiled with JetBrains decompiler
// Type: AutomateCommandCreation.ParamMetaData
// Assembly: AutomateCommandCreation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D80C71A9-7029-489E-8398-6B27AD4E17DC
// Assembly location: D:\Storage\nagelfar@mail.ru\Music\betting-system\Project Tools\AutomateCommandCreation\AutomateCommandCreation.exe

using System.Collections.Generic;

namespace AutomateCommandCreation
{
  public class ParamMetaData
  {
    public static Dictionary<ParamType, ParamTypeMeta> Items { get; private set; }

    static ParamMetaData()
    {
      ParamMetaData.Items = new Dictionary<ParamType, ParamTypeMeta>();
      ParamMetaData.Items.Add(ParamType.Blob, new ParamTypeMeta("varbinary(max)", "AddVarBinaryMaxParam(\"{0}\"{1})", "byte[]", "GetBytes({0})", "(byte[]){0}.Value", ""));
      ParamMetaData.Items.Add(ParamType.Char, new ParamTypeMeta("char({0})", "AddCharParam(\"{0}\", {2}{1})", "string", "GetStringOrDefault({0})", "{0}.Value.ToString()", ""));
      ParamMetaData.Items.Add(ParamType.Date, new ParamTypeMeta("date", "AddDateParam(\"{0}\"{1})", "DateTime", "GetDateTimeOrNull({0})", "{0}.GetDateTimeOrNull()", ""));
      ParamMetaData.Items.Add(ParamType.Decimal, new ParamTypeMeta("decimal({1},{2})", "AddDecimalParam(\"{0}\", {3}, {4}{1})", "decimal", "GetDecimalOrDefault({0})", "{0}.GetDecimalOrDefault()", ""));
      ParamMetaData.Items.Add(ParamType.Double, new ParamTypeMeta("double precision", "AddFloatParam(\"{0}\"{1})", "double", "GetDoubleOrDefault({0})", "{0}.GetDoubleOrDefault()", ""));
      ParamMetaData.Items.Add(ParamType.Float, new ParamTypeMeta("float", "AddRealParam(\"{0}\"{1})", "float", "GetDoubleOrDefault({0})", "{0}.GetDoubleOrDefault()", ""));
      ParamMetaData.Items.Add(ParamType.Int64, new ParamTypeMeta("bigint", "AddBigIntParam(\"{0}\"{1})", "long", "GetInt64OrDefault({0})", "{0}.GetInt64OrDefault()", ""));
      ParamMetaData.Items.Add(ParamType.Integer, new ParamTypeMeta("int", "AddIntParam(\"{0}\"{1})", "int", "GetInt32OrDefault({0})", "{0}.GetInt32OrDefault()", ""));
      ParamMetaData.Items.Add(ParamType.Smallint, new ParamTypeMeta("smallint", "AddSmallIntParam(\"{0}\"{1})", "short", "GetInt16OrDefault({0})", "{0}.GetInt16OrDefault()", ""));
      ParamMetaData.Items.Add(ParamType.Time, new ParamTypeMeta("time", "AddTimeParam(\"{0}\"{1})", "TimeSpan", "GetTimeSpanOrDefault({0})", "{0}.GetTimeSpanOrDefault()", ""));
      ParamMetaData.Items.Add(ParamType.Timestamp, new ParamTypeMeta("datetime2", "AddDateTime2Param(\"{0}\"{1})", "DateTime", "GetDateTimeOrNull({0})", "{0}.GetDateTimeOrNull()", ""));
      ParamMetaData.Items.Add(ParamType.Varchar, new ParamTypeMeta("varchar({0})", "AddVarCharParam(\"{0}\", {2}{1})", "string", "GetStringOrDefault({0})", "{0}.Value.ToString()", ""));
    }
  }
}
