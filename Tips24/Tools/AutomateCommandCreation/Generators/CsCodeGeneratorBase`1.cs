// Decompiled with JetBrains decompiler
// Type: AutomateCommandCreation.Generators.CsCodeGeneratorBase`1
// Assembly: AutomateCommandCreation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D80C71A9-7029-489E-8398-6B27AD4E17DC
// Assembly location: D:\Storage\nagelfar@mail.ru\Music\betting-system\Project Tools\AutomateCommandCreation\AutomateCommandCreation.exe

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomateCommandCreation.Generators
{
  public class CsCodeGeneratorBase<T> : CsCodeGeneratorAbstract where T : ParamDataBase
  {
    public override void AddInputParams(StringBuilder sb, ref int level, IEnumerable<ParamDataBase> queryParams)
    {
      foreach (T queryParam in queryParams)
      {
        sb.Indent(level);
        queryParam.WriteCsParam(sb);
        sb.AppendLine();
      }
    }

    public override void AddOutputParams(StringBuilder sb, ref int level, IEnumerable<ParamDataBase> queryParams)
    {
      foreach (T queryParam in queryParams)
      {
        sb.Indent(level);
        queryParam.WriteCsParam(sb);
        sb.AppendLine();
      }
    }

    public override void GetOutputParams(StringBuilder sb, ref int level, IEnumerable<ParamDataBase> queryParams)
    {
      foreach (T queryParam in queryParams)
      {
        sb.Indent(level);
        queryParam.WriteGetValueFromParam(sb);
        sb.AppendLine();
      }
    }

    public override void GetReaderParams(StringBuilder sb, ref int level, IEnumerable<IEnumerable<ParamDataBase>> resultColumns)
    {
      bool flag = resultColumns.Count<IEnumerable<ParamDataBase>>() > 1;
      int num = 0;
      foreach (IEnumerable<ParamDataBase> resultColumn in resultColumns)
      {
        sb.Indent(level).AppendLine("while (dr.Read())");
        sb.Indent(level).AppendLine("{");
        ++level;
        foreach (T obj in resultColumn)
        {
          sb.Indent(level);
          obj.WriteGetValueFromReader(sb);
          sb.AppendLine();
        }
        --level;
        sb.Indent(level).AppendLine("}");
        if (flag && num < resultColumns.Count<IEnumerable<ParamDataBase>>() - 1)
        {
          sb.Indent(level).AppendLine();
          sb.Indent(level).AppendLine("dr.NextResult();");
          sb.Indent(level).AppendLine();
        }
        ++num;
      }
    }
  }
}
