// Decompiled with JetBrains decompiler
// Type: AutomateCommandCreation.Generators.FbStoredProcCsCodeGenerator
// Assembly: AutomateCommandCreation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D80C71A9-7029-489E-8398-6B27AD4E17DC
// Assembly location: D:\Storage\nagelfar@mail.ru\Music\betting-system\Project Tools\AutomateCommandCreation\AutomateCommandCreation.exe

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomateCommandCreation.Generators
{
  public sealed class FbStoredProcCsCodeGenerator : CsCodeGeneratorBase<ParamData>
  {
    public override void GetReaderParams(StringBuilder sb, ref int level, IEnumerable<IEnumerable<ParamDataBase>> resultColumns)
    {
      sb.Indent(level).AppendLine("while (dr.Read())");
      sb.Indent(level).AppendLine("{");
      ++level;
      foreach (ParamData paramData in resultColumns.First<IEnumerable<ParamDataBase>>())
      {
        sb.Indent(level);
        paramData.WriteGetValueFromReader(sb);
        sb.AppendLine();
      }
      --level;
      sb.Indent(level).AppendLine("}");
    }
  }
}
