// Decompiled with JetBrains decompiler
// Type: AutomateCommandCreation.ParamDataBase
// Assembly: AutomateCommandCreation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D80C71A9-7029-489E-8398-6B27AD4E17DC
// Assembly location: D:\Storage\nagelfar@mail.ru\Music\betting-system\Project Tools\AutomateCommandCreation\AutomateCommandCreation.exe

using System.Text;

namespace AutomateCommandCreation
{
  public abstract class ParamDataBase
  {
    public string ParamName { get; set; }

    public int Precision { get; set; }

    public int Scale { get; set; }

    public int CharLength { get; set; }

    public bool IsOutput { get; set; }

    public int ColumnOrdinal { get; set; }

    public abstract void WriteCsParam(StringBuilder sb);

    public abstract void WriteGetValueFromParam(StringBuilder sb);

    public abstract void WriteGetValueFromReader(StringBuilder sb);

    public abstract void WriteTsqlParam(StringBuilder sb);
  }
}
