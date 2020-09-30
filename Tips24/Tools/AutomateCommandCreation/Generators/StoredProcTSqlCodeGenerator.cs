// Decompiled with JetBrains decompiler
// Type: AutomateCommandCreation.Generators.StoredProcTSqlCodeGenerator
// Assembly: AutomateCommandCreation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D80C71A9-7029-489E-8398-6B27AD4E17DC
// Assembly location: D:\Storage\nagelfar@mail.ru\Music\betting-system\Project Tools\AutomateCommandCreation\AutomateCommandCreation.exe

using System.Collections.Generic;
using System.Text;

namespace AutomateCommandCreation.Generators
{
  public sealed class StoredProcTSqlCodeGenerator
  {
    public static string GenerateTsql(string procName, List<ParamData> inputParams, List<ParamData> outputParams)
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("IF OBJECT_ID ('dbo.").Append(procName).AppendLine("', 'P') IS NOT NULL").Indent(1).Append("DROP PROCEDURE dbo.").AppendLine(procName).AppendLine("GO").AppendLine();
      sb.Append("CREATE PROCEDURE dbo.").Append(procName);
      int level = 1;
      bool flag = false;
      foreach (ParamData inputParam in inputParams)
      {
        if (inputParam.UseAsParam)
        {
          sb.AppendLine().Indent(level);
          inputParam.WriteTsqlParam(sb);
          flag = true;
        }
      }
      foreach (ParamData outputParam in outputParams)
      {
        if (outputParam.UseAsParam)
        {
          sb.AppendLine().Indent(level);
          outputParam.WriteTsqlParam(sb);
          flag = true;
        }
      }
      if (flag)
        sb.Remove(sb.Length - 1, 1);
      sb.AppendLine().AppendLine("AS").AppendLine("BEGIN");
      sb.Indent(1).AppendLine("SET NOCOUNT ON;").AppendLine().AppendLine("END;").AppendLine("GO");
      return sb.ToString();
    }
  }
}
