// Decompiled with JetBrains decompiler
// Type: AutomateCommandCreation.ParamTypeMeta
// Assembly: AutomateCommandCreation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D80C71A9-7029-489E-8398-6B27AD4E17DC
// Assembly location: D:\Storage\nagelfar@mail.ru\Music\betting-system\Project Tools\AutomateCommandCreation\AutomateCommandCreation.exe

namespace AutomateCommandCreation
{
  public class ParamTypeMeta
  {
    public string TsqlType;
    public string AddParamMethod;
    public string AddParamMaxMethod;
    public string CsType;
    public string DrMethod;
    public string GetParamValue;

    public ParamTypeMeta(string tsqlType, string addParamMethod, string csType, string drMethod, string getParamValue, string addParamMaxMethod = "")
    {
      this.TsqlType = tsqlType;
      this.AddParamMethod = addParamMethod;
      this.AddParamMaxMethod = addParamMaxMethod;
      this.CsType = csType;
      this.DrMethod = drMethod;
      this.GetParamValue = getParamValue;
    }
  }
}
