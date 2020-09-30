// Decompiled with JetBrains decompiler
// Type: AutomateCommandCreation.SqlParamData
// Assembly: AutomateCommandCreation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D80C71A9-7029-489E-8398-6B27AD4E17DC
// Assembly location: D:\Storage\nagelfar@mail.ru\Music\betting-system\Project Tools\AutomateCommandCreation\AutomateCommandCreation.exe

using System;
using System.Data;
using System.Text;

namespace AutomateCommandCreation
{
  public class SqlParamData : ParamDataBase
  {
    public string TypeDescription
    {
      get
      {
        switch (this.Type)
        {
          case SqlDbType.VarBinary:
          case SqlDbType.Binary:
            return string.Format("{0}({1})", (object) this.Type, this.CharLength == -1 ? (object) "MAX" : (object) this.CharLength.ToString());
          case SqlDbType.VarChar:
          case SqlDbType.Char:
          case SqlDbType.NChar:
          case SqlDbType.NVarChar:
            return string.Format("{0}({1})", (object) this.Type, this.CharLength == -1 ? (object) "MAX" : (object) this.CharLength.ToString());
          case SqlDbType.Time:
          case SqlDbType.DateTime2:
          case SqlDbType.DateTimeOffset:
            return string.Format("{0}({1})", (object) this.Type, (object) this.Scale);
          case SqlDbType.Decimal:
            return string.Format("{0}({1},{2})", (object) this.Type, (object) this.Precision, (object) this.Scale);
          case SqlDbType.Float:
            return string.Format("{0}({1})", (object) this.Type, (object) this.Precision);
          default:
            return this.Type.ToString();
        }
      }
    }

    public SqlDbType Type { get; set; }

    public SqlParamData(string paramName, SqlDbType type, int precision, int scale, int charLength)
    {
      this.ParamName = paramName;
      this.Type = type;
      this.Precision = precision;
      this.Scale = scale;
      this.CharLength = charLength;
      this.IsOutput = false;
    }

    public SqlParamData(string paramName, SqlDbType type, int precision, int scale, int charLength, int columnOrdinal)
      : this(paramName, type, precision, scale, charLength)
    {
      this.ColumnOrdinal = columnOrdinal;
    }

    public SqlParamData(string paramName, SqlDbType type, int precision, int scale, int charLength, bool isOutput)
      : this(paramName, type, precision, scale, charLength)
    {
      this.IsOutput = isOutput;
    }

    public override void WriteCsParam(StringBuilder sb)
    {
      ParamTypeMeta paramTypeMeta = SqlParamMetaData.Items[this.Type];
      if (this.IsOutput)
        sb.Append("SqlParameter ").Append(this.ParamName).Append("Param = ");
      string str = this.IsOutput ? "" : ", request." + this.ParamName;
      string format = this.Type != SqlDbType.VarChar && this.Type != SqlDbType.NVarChar || this.CharLength != -1 ? paramTypeMeta.AddParamMethod : paramTypeMeta.AddParamMaxMethod;
      sb.Append("cmd.").AppendFormat(format, (object) ("@" + this.ParamName), (object) str, (object) this.CharLength, (object) this.Precision, (object) this.Scale);
      if (this.IsOutput)
        sb.Append(".Output()");
      sb.Append(";");
    }

    public override void WriteGetValueFromParam(StringBuilder sb)
    {
      ParamTypeMeta paramTypeMeta = SqlParamMetaData.Items[this.Type];
      sb.Append(paramTypeMeta.CsType).Append(" ").Append(this.ParamName).Append(" = ").AppendFormat(paramTypeMeta.GetParamValue, (object) (this.ParamName + "Param")).Append(";");
    }

    public override void WriteGetValueFromReader(StringBuilder sb)
    {
      ParamTypeMeta paramTypeMeta = SqlParamMetaData.Items[this.Type];
      object obj = string.IsNullOrWhiteSpace(this.ParamName) ? (object) this.ColumnOrdinal : (object) ("\"" + this.ParamName + "\"");
      sb.Append(paramTypeMeta.CsType).Append(" ").Append(this.ParamName).Append(" = dr.").AppendFormat(paramTypeMeta.DrMethod, obj).Append(";");
    }

    public override void WriteTsqlParam(StringBuilder sb)
    {
      throw new NotImplementedException();
    }
  }
}
