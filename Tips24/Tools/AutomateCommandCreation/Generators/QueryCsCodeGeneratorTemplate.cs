// Decompiled with JetBrains decompiler
// Type: AutomateCommandCreation.Generators.QueryCsCodeGeneratorTemplate
// Assembly: AutomateCommandCreation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D80C71A9-7029-489E-8398-6B27AD4E17DC
// Assembly location: D:\Storage\nagelfar@mail.ru\Music\betting-system\Project Tools\AutomateCommandCreation\AutomateCommandCreation.exe

using System.Collections.Generic;
using System.Text;

namespace AutomateCommandCreation.Generators
{
  public sealed class QueryCsCodeGeneratorTemplate
  {
    private CsCodeGeneratorAbstract _methodsContainer;

    public bool NeedReader { get; private set; }

    public bool OutputParamAdded { get; private set; }

    public bool InputParamAdded { get; private set; }

    public bool RequestResponse { get; private set; }

    public bool Connection { get; private set; }

    public bool ReturnValue { get; private set; }

    public bool Transaction { get; private set; }

    public bool IsStoredProc { get; private set; }

    public QueryCsCodeGeneratorTemplate(CsCodeGeneratorAbstract methodsContainer, bool haveInputParam, bool haveOutputParam, bool needReader, bool requestResponse, bool connection, bool returnValue, bool transaction, bool isStoredProc)
    {
      this.NeedReader = needReader;
      this.OutputParamAdded = haveOutputParam || returnValue;
      this.InputParamAdded = haveInputParam;
      this.RequestResponse = requestResponse;
      this.Connection = connection;
      this.ReturnValue = returnValue;
      this.Transaction = transaction;
      this.IsStoredProc = isStoredProc;
      this._methodsContainer = methodsContainer;
    }

    public string GenerateCommand(string querySql, IEnumerable<ParamDataBase> inputParams, IEnumerable<ParamDataBase> outputParams, IEnumerable<IEnumerable<ParamDataBase>> resultColumns)
    {
      StringBuilder sb = new StringBuilder();
      int level1 = 0;
      if (this.RequestResponse)
      {
        sb.Indent(level1).AppendLine("Request request = context.GetRequest<Request>();");
        sb.Indent(level1).AppendLine("if (request == null)");
        sb.Indent(level1).AppendLine("{");
        int level2 = level1 + 1;
        sb.Indent(level2).AppendLine("await context.SendErrorJsonAsync(\"Неверные данные\");");
        sb.Indent(level2).AppendLine("return;");
        level1 = level2 - 1;
        sb.Indent(level1).AppendLine("}").AppendLine();
        sb.Indent(level1).AppendLine("Response response = new Response(context.Command);").AppendLine();
      }
      if (this.Connection)
      {
        sb.Indent(level1).AppendLine("using (SqlConnection conn = SqlServer.GetConnection())");
        sb.Indent(level1).AppendLine("{");
        int level2 = level1 + 1;
        sb.Indent(level2).AppendLine("if (!await conn.SafeOpenAsync())");
        sb.Indent(level2).AppendLine("{");
        int level3 = level2 + 1;
        sb.Indent(level3).AppendLine("await context.SendErrorJsonAsync(\"Ошибка коннекта к базе\");");
        sb.Indent(level3).AppendLine("return;");
        int level4 = level3 - 1;
        sb.Indent(level4).AppendLine("}").AppendLine().AppendLine();
        sb.Indent(level4).AppendLine("try");
        sb.Indent(level4).AppendLine("{");
        level1 = level4 + 1;
      }
      if (this.Transaction)
      {
        sb.Indent(level1).AppendLine("using (SqlTransaction transaction = conn.BeginTransaction())");
        sb.Indent(level1).AppendLine("{");
        ++level1;
      }
      string empty = string.Empty;
      string str;
      if (this.IsStoredProc)
        str = "using (SqlCommand cmd = SqlServer.GetSpCommand(\"" + querySql + "\",  conn" + (this.Transaction ? ", transaction" : string.Empty) + "))";
      else
        str = "using (SqlCommand cmd = new SqlCommand(\"" + querySql + "\", conn" + (this.Transaction ? ", transaction" : string.Empty) + "))";
      sb.Indent(level1).AppendLine(str);
      sb.Indent(level1).AppendLine("{");
      int level5 = level1 + 1;
      this._methodsContainer.AddInputParams(sb, ref level5, inputParams);
      if (this.InputParamAdded)
        sb.AppendLine();
      this._methodsContainer.AddOutputParams(sb, ref level5, outputParams);
      if (this.ReturnValue)
        sb.Indent(level5).AppendLine("SqlParameter retValParam = cmd.AddReturnValue();");
      if (this.OutputParamAdded)
        sb.AppendLine();
      if (this.NeedReader)
      {
        sb.Indent(level5).AppendLine("using (SqlDataReader dr = await cmd.ExecuteReaderAsync())");
        sb.Indent(level5).AppendLine("{");
        ++level5;
        this._methodsContainer.GetReaderParams(sb, ref level5, resultColumns);
        --level5;
        sb.Indent(level5).AppendLine("}");
      }
      else
        sb.Indent(level5).AppendLine("await cmd.ExecuteNonQueryAsync();");
      if (this.OutputParamAdded)
        sb.AppendLine();
      if (this.ReturnValue)
        sb.Indent(level5).AppendLine("int retVal = retValParam.GetInt32OrDefault();");
      this._methodsContainer.GetOutputParams(sb, ref level5, outputParams);
      --level5;
      sb.Indent(level5).AppendLine("}");
      if (this.Transaction)
      {
        sb.AppendLine();
        sb.Indent(level5).AppendLine("transaction.Commit();");
        --level5;
        sb.Indent(level5).AppendLine("}");
      }
      if (this.Connection)
      {
        --level5;
        sb.Indent(level5).AppendLine("}");
        sb.Indent(level5).AppendLine("catch (Exception e)");
        sb.Indent(level5).AppendLine("{").AppendLine();
        sb.Indent(level5).AppendLine("}");
        --level5;
        sb.Indent(level5).Append("}");
      }
      return sb.ToString();
    }
  }
}
