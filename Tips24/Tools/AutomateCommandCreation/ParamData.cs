// Decompiled with JetBrains decompiler
// Type: AutomateCommandCreation.ParamData
// Assembly: AutomateCommandCreation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D80C71A9-7029-489E-8398-6B27AD4E17DC
// Assembly location: D:\Storage\nagelfar@mail.ru\Music\betting-system\Project Tools\AutomateCommandCreation\AutomateCommandCreation.exe

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace AutomateCommandCreation
{
  public class ParamData : ParamDataBase, INotifyPropertyChanged
  {
    private bool _useAsParam;
    private bool _useAsOutputColumn;

    public string InitialParamName { get; set; }

    public bool UseAsParam
    {
      get
      {
        return this._useAsParam;
      }
      set
      {
        if (this._useAsParam == value)
          return;
        this._useAsParam = value;
        this.OnPropertyChanged("UseAsParam");
      }
    }

    public bool UseAsOutputColumn
    {
      get
      {
        return this._useAsOutputColumn;
      }
      set
      {
        if (this._useAsOutputColumn == value)
          return;
        this._useAsOutputColumn = value;
        this.OnPropertyChanged("UseAsOutputColumn");
      }
    }

    public string OutputColumnName { get; set; }

    public ParamType Type { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    public ParamData(string paramName, bool isOutput, ParamType type, int precision, int scale, int charLength)
    {
      this.OutputColumnName = this.InitialParamName = paramName;
      this.ParamName = this.GetNormalizedName(false);
      this.IsOutput = isOutput;
      this.Type = type;
      this.Precision = precision;
      this.Scale = scale;
      this.CharLength = charLength;
      if (this.IsOutput)
        return;
      this.UseAsParam = true;
    }

    public void NormalizeParamName(bool withUnderScore)
    {
      this.ParamName = this.GetNormalizedName(withUnderScore);
    }

    private string GetNormalizedName(bool withUnderScore)
    {
      string[] strArray = this.InitialParamName.Split('_');
      StringBuilder stringBuilder = new StringBuilder();
      for (int index1 = 0; index1 < strArray.Length; ++index1)
      {
        string str = strArray[index1];
        if (withUnderScore && index1 > 0)
          stringBuilder.Append("_");
        for (int index2 = 0; index2 < str.Length; ++index2)
          stringBuilder.Append(index2 != 0 || index1 <= 0 || withUnderScore ? char.ToLower(str[index2]) : char.ToUpper(str[index2]));
      }
      return stringBuilder.ToString();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    }

    public override void WriteCsParam(StringBuilder sb)
    {
      ParamTypeMeta paramTypeMeta = ParamMetaData.Items[this.Type];
      if (this.IsOutput)
        sb.Append("SqlParameter ").Append(this.ParamName).Append("Param = ");
      string str = this.IsOutput ? "" : ", request." + this.ParamName;
      sb.Append("cmd.").AppendFormat(paramTypeMeta.AddParamMethod, (object) ("@" + this.ParamName), (object) str, (object) this.CharLength, (object) this.Precision, (object) this.Scale);
      if (this.IsOutput)
        sb.Append(".Output()");
      sb.Append(";");
    }

    public override void WriteGetValueFromParam(StringBuilder sb)
    {
      ParamTypeMeta paramTypeMeta = ParamMetaData.Items[this.Type];
      sb.Append(paramTypeMeta.CsType).Append(" ").Append(this.ParamName).Append(" = ").AppendFormat(paramTypeMeta.GetParamValue, (object) (this.ParamName + "Param")).Append(";");
    }

    public override void WriteGetValueFromReader(StringBuilder sb)
    {
      ParamTypeMeta paramTypeMeta = ParamMetaData.Items[this.Type];
      string str = "\"" + this.OutputColumnName + "\"";
      sb.Append(paramTypeMeta.CsType).Append(" ").Append(this.ParamName).Append(" = dr.").AppendFormat(paramTypeMeta.DrMethod, (object) str).Append(";");
    }

    public override void WriteTsqlParam(StringBuilder sb)
    {
      string tsqlType = ParamMetaData.Items[this.Type].TsqlType;
      sb.Append("@").Append(this.ParamName).Append(" ").AppendFormat(tsqlType, (object) this.CharLength, (object) this.Precision, (object) this.Scale);
      if (this.IsOutput)
        sb.Append(" OUTPUT");
      sb.Append(",");
    }
  }
}
