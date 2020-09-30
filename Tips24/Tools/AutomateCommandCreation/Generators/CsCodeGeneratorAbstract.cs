// Decompiled with JetBrains decompiler
// Type: AutomateCommandCreation.Generators.CsCodeGeneratorAbstract
// Assembly: AutomateCommandCreation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D80C71A9-7029-489E-8398-6B27AD4E17DC
// Assembly location: D:\Storage\nagelfar@mail.ru\Music\betting-system\Project Tools\AutomateCommandCreation\AutomateCommandCreation.exe

using System.Collections.Generic;
using System.Text;

namespace AutomateCommandCreation.Generators
{
  public abstract class CsCodeGeneratorAbstract
  {
    public abstract void AddInputParams(StringBuilder sb, ref int level, IEnumerable<ParamDataBase> queryParams);

    public abstract void AddOutputParams(StringBuilder sb, ref int level, IEnumerable<ParamDataBase> queryParams);

    public abstract void GetReaderParams(StringBuilder sb, ref int level, IEnumerable<IEnumerable<ParamDataBase>> resultColumns);

    public abstract void GetOutputParams(StringBuilder sb, ref int level, IEnumerable<ParamDataBase> queryParams);
  }
}
